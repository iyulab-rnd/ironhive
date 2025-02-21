﻿using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Raggle.Abstractions;
using Raggle.Abstractions.Memory;
using Raggle.Core.Memory.Handlers;
using Raggle.Server.Configurations;
using Raggle.Server.Data;
using Raggle.Server.Entities;
using System.Text.Json.Nodes;

namespace Raggle.Server.Services;

public class MemoryService
{
    private readonly string _id;
    private readonly RaggleDbContext _db;
    private readonly IMemoryService _memory;

    public MemoryService(RaggleDbContext dbContext, IMemoryService memory, string serviceId)
    {
        _id = serviceId;
        _db = dbContext;
        _memory = memory;
    }

    #region Collection

    public async Task<IEnumerable<CollectionEntity>> FindCollectionsAsync(
        string? name = null,
        int limit = 10,
        int skip = 0,
        string order = "desc")
    {
        var query = _db.Collections.AsQueryable();

        if (name != null)
            query = query.Where(c => c.Name.Contains(name));

        if (order == "desc")
            query = query.OrderByDescending(c => c.CreatedAt);
        else
            query = query.OrderBy(c => c.CreatedAt);

        var collections = await query.Skip(skip).Take(limit).ToArrayAsync();
        return collections;
    }

    public async Task<CollectionEntity> GetCollectionAsync(string collectionId)
    {
        var collection = await _db.Collections.FindAsync(collectionId);
        return collection ?? throw new InvalidOperationException("Collection not found.");
    }

    public async Task<CollectionEntity> UpsertCollectionAsync(CollectionEntity collection)
    {
        var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var exists = await _db.Collections.AnyAsync(c => c.Id == collection.Id);
            if (exists)
            {
                collection.LastUpdatedAt = DateTime.UtcNow;
                _db.Collections.Update(collection);
            }
            else
            {
                _db.Collections.Add(collection);
                await _memory.CreateCollectionAsync(
                    collection.Id,
                    collection.EmbedModel);
            }
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return collection;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task DeleteCollectionAsync(string collectionId)
    {
        var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var collection = await _db.Collections.FindAsync(collectionId)
                ?? throw new InvalidOperationException("Collection not found.");

            _db.Collections.Remove(collection);
            await _memory.DeleteCollectionAsync(collection.Id);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    #endregion

    #region Document

    public async Task<IEnumerable<DocumentEntity>> FindDocumentsAsync(
        string collectionId,
        string? fileName = null,
        int limit = 10,
        int skip = 0,
        string order = "desc")
    {
        var query = _db.Collections
            .Where(c => c.Id == collectionId)
            .Include(c => c.Documents)
            .SelectMany(c => c.Documents!)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(fileName))
            query = query.Where(c => c.FileName.Contains(fileName));

        query = (order.ToLowerInvariant()) switch
        {
            "desc" => query.OrderByDescending(c => c.LastUpdatedAt),
            _ => query.OrderBy(c => c.LastUpdatedAt)
        };

        var documents = await query.Skip(skip).Take(limit).ToArrayAsync();
        return documents;
    }

    public async Task<DocumentEntity> UploadDocumentAsync(
        string collectionId, 
        IFormFile file, 
        IEnumerable<string>? tags = null)
    {
        var collection = await _db.Collections.FindAsync(collectionId)
            ?? throw new InvalidOperationException("Collection not found.");

        var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var document = new DocumentEntity
            {
                CollectionId = collectionId,
                FileName = file.FileName,
                FileSize = file.Length,
                ContentType = file.ContentType,
                Tags = tags,
            };

            await _db.Documents.AddAsync(document);

            var data = file.OpenReadStream();
            await _memory.UploadDocumentAsync(
                collectionName: collection.Id,
                documentId: document.Id,
                fileName: document.FileName,
                data: data);

            _ = _memory.MemorizeDocumentAsync(
                collectionName: collection.Id,
                documentId: document.Id,
                fileName: document.FileName,
                tags: document.Tags?.ToArray(),
                steps: PreparePipelineSteps(collection),
                options: PreparePipelineOptions(collection));

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return document;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task DeleteDocumentAsync(string collectionId, string documentId)
    {
        var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var document = await _db.Documents.FindAsync(documentId)
            ?? throw new InvalidOperationException("Document not found.");

            _db.Documents.Remove(document);
            await _memory.DeleteDocumentAsync(collectionId, documentId);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task<IEnumerable<ScoredVectorPoint>> SearchDocumentAsync(string collectionId, string query)
    {
        var collection = await _db.Collections.FindAsync(collectionId)
            ?? throw new InvalidOperationException("Collection not found.");

        return await _memory.SearchSimilarVectorsAsync(
            collectionName: collection.Id,
            embedModel: collection.EmbedModel,
            query: query);
    }

    #endregion

    private static string[] PreparePipelineSteps(CollectionEntity collection)
    {
        var steps = new List<string>
        {
            DefaultServiceKeys.Decoding,
            DefaultServiceKeys.Chunking
        };

        if (collection.HandlerOptions?.ContainsKey(DefaultServiceKeys.Summarizing) == true)
            steps.Add(DefaultServiceKeys.Summarizing);

        if (collection.HandlerOptions?.ContainsKey(DefaultServiceKeys.Dialogue) == true)
            steps.Add(DefaultServiceKeys.Dialogue);

        steps.Add(DefaultServiceKeys.Embeddings);
        return steps.ToArray();
    }

    private static IDictionary<string, object> PreparePipelineOptions(CollectionEntity collection)
    {
        var options = new Dictionary<string, object>();

        if (collection.HandlerOptions?.ContainsKey(DefaultServiceKeys.Chunking) == true)
            options[DefaultServiceKeys.Chunking] = collection.HandlerOptions[DefaultServiceKeys.Chunking];

        if (collection.HandlerOptions?.ContainsKey(DefaultServiceKeys.Summarizing) == true)
            options[DefaultServiceKeys.Summarizing] = collection.HandlerOptions[DefaultServiceKeys.Summarizing];

        if (collection.HandlerOptions?.ContainsKey(DefaultServiceKeys.Dialogue) == true)
            options[DefaultServiceKeys.Dialogue] = collection.HandlerOptions[DefaultServiceKeys.Dialogue];

        options[DefaultServiceKeys.Embeddings] = new EmbeddingsHandler.Options
        {
            Model = collection.EmbedModel
        };

        return options;
    }
}
