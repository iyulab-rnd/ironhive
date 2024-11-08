﻿using System.Net.Http.Json;
using System.Text.Json;
using Raggle.Connector.OpenAI.Configurations;

namespace Raggle.Connector.OpenAI.Base;

internal abstract class OpenAIClientBase
{
    protected readonly HttpClient _client;
    protected readonly JsonSerializerOptions _jsonOptions;

    protected OpenAIClientBase(OpenAIConfig config)
    {
        _client = CreateHttpClient(config);
        _jsonOptions = config.JsonOptions;
    }

    protected OpenAIClientBase(string apiKey)
    {
        var defaultOptions = new OpenAIConfig { ApiKey = apiKey };
        _client = CreateHttpClient(defaultOptions);
        _jsonOptions = defaultOptions.JsonOptions;
    }

    /// <summary>
    /// Gets the list of OpenAI models.
    /// </summary>
    protected async Task<IEnumerable<OpenAIModel>> GetModelsAsync(CancellationToken cancellationToken)
    {
        var jsonDocument = await _client.GetFromJsonAsync<JsonDocument>(OpenAIConstants.GetModelsPath, cancellationToken);
        var models = jsonDocument?.RootElement.GetProperty("data").Deserialize<IEnumerable<OpenAIModel>>();

        return models?.OrderByDescending(m => m.Created).ToArray() ?? [];
    }

    private static HttpClient CreateHttpClient(OpenAIConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.ApiKey))
            throw new ArgumentException("OpenAI API key is required.");

        var client = new HttpClient
        {
            BaseAddress = string.IsNullOrWhiteSpace(config.EndPoint)
                ? new Uri(OpenAIConstants.DefaultEndPoint)
                : new Uri(config.EndPoint.EndsWith('/') ? config.EndPoint : $"{config.EndPoint}/")
        };

        if (!string.IsNullOrWhiteSpace(config.ApiKey))
            client.DefaultRequestHeaders.Add(OpenAIConstants.ApiKeyHeaderName, string.Format(OpenAIConstants.ApiKeyHeaderValue, config.ApiKey));

        if (!string.IsNullOrWhiteSpace(config.Organization))
            client.DefaultRequestHeaders.Add(OpenAIConstants.OrganizationHeaderName, config.Organization);

        if (!string.IsNullOrWhiteSpace(config.Project))
            client.DefaultRequestHeaders.Add(OpenAIConstants.ProjectHeaderName, config.Project);

        return client;
    }
}
