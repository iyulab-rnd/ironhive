﻿using Raggle.Abstractions.JsonConverters;
using System.Text.Json.Serialization;

namespace Raggle.Driver.OpenAI.ChatCompletion.Models;

internal class StreamingChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string? ID { get; set; }

    [JsonPropertyName("choices")]
    public ChoiceDelta[]? Choices { get; set; }

    [JsonPropertyName("created")]
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime Created { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; set; }

    /// <summary>
    /// always "chat.completion"
    /// </summary>
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("usage")]
    public TokenUsage? Usage { get; set; }
}
