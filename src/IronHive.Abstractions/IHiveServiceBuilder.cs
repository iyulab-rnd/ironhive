﻿using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Tools;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents a builder for configuring the hive services.
/// </summary>
public interface IHiveServiceBuilder
{
    /// <summary>
    /// Adds a chat completion connector to the hive service store.
    /// </summary>
    IHiveServiceBuilder AddChatCompletionConnector(
        string serviceKey, 
        IChatCompletionConnector connector);

    /// <summary>
    /// Adds an embedding connector to the hive service store.
    /// </summary>
    IHiveServiceBuilder AddEmbeddingConnector(
        string serviceKey, 
        IEmbeddingConnector connector);

    /// <summary>
    /// Adds a tool handler to the service collection.
    /// </summary>
    IHiveServiceBuilder AddToolHandler<TService>(
        string serviceKey, 
        ServiceLifetime lifetime = ServiceLifetime.Singleton, 
        Func<IServiceProvider, object?, TService>? implementationFactory = null)
        where TService : class, IToolHandler;

    /// <summary>
    /// Adds a queue storage to the service collection as a singleton.
    /// </summary>
    IHiveServiceBuilder WithQueueStorage(IQueueStorage storage);

    /// <summary>
    /// Adds a cache storage to the service collection as a singleton.
    /// </summary>
    IHiveServiceBuilder WithPipelineStorage(IPipelineStorage storage);

    /// <summary>
    /// Adds a vector storage to the service collection as a singleton.
    /// </summary>
    IHiveServiceBuilder WithVectorStorage(IVectorStorage storage);

    /// <summary>
    /// Adds a pipline handler to the service collection.
    /// </summary>
    IHiveServiceBuilder AddPipelineHandler<TService>(
        string serviceKey, 
        ServiceLifetime lifetime = ServiceLifetime.Singleton, 
        Func<IServiceProvider, object?, TService>? implementationFactory = null) 
        where TService : class, IPipelineHandler;

    /// <summary>
    /// Adds multiple file decoders to the service collection as a singleton.
    /// </summary>
    IHiveServiceBuilder AddFileDecoder(IFileDecoder decoder);

    /// <summary>
    /// Create the hive mind object.
    /// </summary>
    IHiveMind BuildHiveMind();
}
