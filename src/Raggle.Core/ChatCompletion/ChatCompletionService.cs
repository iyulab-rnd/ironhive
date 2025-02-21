﻿using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.ChatCompletion.Messages;
using System.Runtime.CompilerServices;

namespace Raggle.Core.ChatCompletion;

public class ChatCompletionService : IChatCompletionService
{
    private readonly IServiceProvider _service;
    private readonly IServiceModelConverter _converter;

    public ChatCompletionService(IServiceProvider service)
    {
        _service = service;
        _converter = service.GetRequiredService<IServiceModelConverter>();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatCompletionModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var services = _service.GetKeyedServices<IChatCompletionAdapter>(KeyedService.AnyKey);
        var models = new List<ChatCompletionModel>();

        foreach (var service in services)
        {
            var serviceKey = ServiceKeyRegistry.Get(service.GetType());
            var serviceModels = await service.GetModelsAsync(cancellationToken);

            models.AddRange(serviceModels.Select(x => new ChatCompletionModel
            {
                Model = _converter.Format(serviceKey, x.Model),
            }));
        }
        return models;
    }

    /// <inheritdoc />
    public async Task<ChatCompletionResponse<IMessage>> GenerateMessageAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var (serviceKey, model) = _converter.Parse(request.Model);
        var adapter = _service.GetRequiredKeyedService<IChatCompletionAdapter>(serviceKey);
        request.Model = model;

        return await adapter.GenerateMessageAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionResponse<IMessageContent>> GenerateStreamingMessageAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (serviceKey, model) = _converter.Parse(request.Model);
        var adapter = _service.GetRequiredKeyedService<IChatCompletionAdapter>(serviceKey);
        request.Model = model;

        await foreach(var res in adapter.GenerateStreamingMessageAsync(request, cancellationToken))
        {
            yield return res;
        }
    }
}
