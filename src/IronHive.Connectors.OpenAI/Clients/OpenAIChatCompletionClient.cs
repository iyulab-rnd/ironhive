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
        request.Stream = true;
        request.StreamOptions = new StreamOptions { InCludeUsage = true };
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var _request = new HttpRequestMessage(HttpMethod.Post, OpenAIConstants.PostChatCompletionPath.RemovePreffix('/'));
        _request.Content = content;
        using var response = await Client.SendAsync(_request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        //response.EnsureSuccessStatusCode();

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(inner: null,
                message: $"Request failed with status code {response.StatusCode}: {error}",
                statusCode: response.StatusCode);
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);

            Console.WriteLine(line);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            else if (line.StartsWith("data:"))
            {
                var data = line.Substring("data:".Length).Trim();
                if (!data.StartsWith('{') || !data.EndsWith('}'))
                    continue;

                var message = JsonSerializer.Deserialize<StreamingChatCompletionResponse>(data, JsonOptions);
                if (message != null)
                {
                    yield return message;
                }
            }
            // 특정 플랫폼의 경우
            else if (line.StartsWith("error:"))
            {
                var data = line.Substring("error:".Length).Trim();
                throw new InvalidOperationException(data);
            }
            else
            {
                continue;
            }
        }
    }
}
