﻿namespace IronHive.Abstractions.Embedding;

public interface IEmbeddingConnector
{
    /// <summary>
    /// Gets the available embedding models.
    /// </summary>
    Task<IEnumerable<EmbeddingModel>> GetModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the embedding model.
    /// </summary>
    Task<EmbeddingModel> GetModelAsync(
        string model,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates multiple embeddings for the given request.
    /// </summary>
    Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string model,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default);
}
