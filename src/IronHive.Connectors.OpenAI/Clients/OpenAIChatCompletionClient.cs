﻿using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Runtime.CompilerServices;
using IronHive.Connectors.OpenAI.ChatCompletion;

namespace IronHive.Connectors.OpenAI.Clients;

internal class OpenAIChatCompletionClient : OpenAIClientBase
{
    internal OpenAIChatCompletionClient(string apiKey) : base(apiKey) { }

    internal OpenAIChatCompletionClient(OpenAIConfig config) : base(config) { }

    internal async Task<ChatCompletionResponse> PostChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        request.Stream = false;
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await Client.PostAsync(OpenAIConstants.PostChatCompletionPath.RemovePreffix('/'), content, cancellationToken);
        response.EnsureSuccessStatusCode();
        var message = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        return message;
    }

    internal async IAsyncEnumerable<StreamingChatCompletionResponse> PostStreamingChatCompletionAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        request.Messages = request.Messages.Select(m =>
        {
            if (m is DeveloperMessage dm)
            {
                m = new SystemMessage
                {
                    Content = dm.Content
                };
            }
            return m;
        }).ToArray();
        request.Stream = true;
        request.StreamOptions = new StreamOptions { InCludeUsage = true };
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var _request = new HttpRequestMessage(HttpMethod.Post, OpenAIConstants.PostChatCompletionPath.RemovePreffix('/'));
        _request.Content = content;
        using var response = await Client.SendAsync(_request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data"))
                continue;

            var data = line.Substring("data:".Length).Trim();
            if (!data.StartsWith('{') || !data.EndsWith('}'))
                continue;

            Console.WriteLine(data);
            var message = JsonSerializer.Deserialize<StreamingChatCompletionResponse>(data, JsonOptions);
            if (message != null)
            {
                yield return message;
            }
        }
    }
}
