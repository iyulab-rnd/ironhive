﻿using IronHive.Abstractions;
using IronHive.Abstractions.Embedding;

namespace IronHive.Core.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly IHiveServiceStore _store;

    public EmbeddingService(IHiveServiceStore store)
    {
        _store = store;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<float>> EmbedAsync(
        string provider,
        string model,
        string input,
        CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetService<IEmbeddingProvider>(provider, out var connector))
            throw new KeyNotFoundException($"Service key '{provider}' not found.");

        return await connector.EmbedAsync(model, input, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string provider,
        string model,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetService<IEmbeddingProvider>(provider, out var connector))
            throw new KeyNotFoundException($"Service key '{provider}' not found.");

        return await connector.EmbedBatchAsync(model, inputs, cancellationToken);
    }
}
