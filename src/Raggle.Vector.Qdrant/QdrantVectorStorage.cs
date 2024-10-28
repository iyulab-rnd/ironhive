﻿using Google.Protobuf.Collections;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;
using Raggle.Abstractions.Memory;
using System.Net.Http.Headers;

namespace Raggle.Vector.Qdrant;

public class QdrantVectorStorage : IVectorStorage
{
    private const string DefaultVectorsName = "default_vector";
    private readonly QdrantClient _client;

    public QdrantVectorStorage(QdrantConfig config)
    {
        _client = CreateQdrantClient(config);
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<IEnumerable<string>> GetCollectionListAsync(CancellationToken cancellationToken = default)
    {
        return await _client.ListCollectionsAsync(cancellationToken);
    }

    public async Task<bool> ExistCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        return await _client.CollectionExistsAsync(collectionName, cancellationToken);
    }

    public async Task CreateCollectionAsync(string collectionName, ulong vectorSize, CancellationToken cancellationToken = default)
    {
        if (await ExistCollectionAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"collection {collectionName} already exist");

        var vectorParams = new VectorParams
        {
            Datatype = Datatype.Float32,
            Distance = Distance.Cosine,
            OnDisk = true,
            Size = vectorSize
        };
        var vectorConfig = new VectorParamsMap { Map = { [DefaultVectorsName] = vectorParams } };
        await _client.CreateCollectionAsync(
            collectionName: collectionName,
            vectorsConfig: vectorConfig,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        if (!await ExistCollectionAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"collection {collectionName} is not exist");

        await _client.DeleteCollectionAsync(
            collectionName: collectionName, 
            cancellationToken: cancellationToken);
    }

    public async Task UpsertVectorsAsync(string collectionName, IEnumerable<VectorPoint> points, CancellationToken cancellationToken = default)
    {
        var pointStructs = new List<PointStruct>();
        foreach(var point in points)
        {
            pointStructs.Add(new PointStruct
            {
                Id = point.VectorId,
                Vectors = new Dictionary<string, float[]>
                {
                    [DefaultVectorsName] = point.Vectors,
                },
                Payload =
                {
                    ["text"] = point.Text,
                    ["documentId"] = point.DocumentId,
                    ["tags"] = point.Tags ?? [],
                }
            });
        }

        await _client.UpsertAsync(
            collectionName: collectionName,
            points: pointStructs,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteVectorsAsync(string collectionName, string documentId, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync(
            collectionName: collectionName,
            filter: MatchKeyword("documentId", documentId),
            cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<ScoredVectorPoint>> SearchVectorsAsync(string collectionName, float[] input, float minScore = 0.0f, ulong limit = 5, MemoryFilter? filter = null, CancellationToken cancellationToken = default)
    {
        Filter? condition = null;
        if (filter != null)
        {
            var conditions = new List<Condition>();
            if (filter.DocumentIds.Count != 0)
            {
                conditions.Add(Match("documentId", filter.DocumentIds));
            }
            if (filter.Tags.Count != 0)
            {
                var tagsFilters = filter.Tags.Select(tag => MatchKeyword("tags", tag));
                var tagsCondition = tagsFilters.Aggregate((current, next) => current & next);
                conditions.Add(tagsCondition);
            }
            if (conditions.Count != 0)
            {
                condition = conditions.Aggregate((current, next) => current & next);
            }
        }

        var results = await _client.SearchAsync(
            collectionName: collectionName,
            vector: input,
            filter: condition,
            limit: limit,
            payloadSelector: true,
            vectorsSelector: false,
            scoreThreshold: minScore,
            vectorName: DefaultVectorsName,
            cancellationToken: cancellationToken);

        var rankedPoints = new List<ScoredVectorPoint>();
        foreach (var result in results)
        {
            var documentId = result.Payload.GetValueOrDefault("documentId")?.StringValue;
            var text = result.Payload.GetValueOrDefault("text")?.StringValue;
            if (documentId == null || text == null)
                continue;

            rankedPoints.Add(new ScoredVectorPoint
            {
                DocumentId = documentId,
                Score = result.Score,
                Text = text,
            });
        }
        return rankedPoints.OrderByDescending(p => p.Score);
    }

    #region Private Methods

    private QdrantClient CreateQdrantClient(QdrantConfig config)
    {
        return new QdrantClient(
            host: config.Host,
            port: config.Port,
            https: config.Https,
            apiKey: config.ApiKey,
            grpcTimeout: config.GrpcTimeout);
    }

    #endregion
}
