﻿using System.Text.Json.Serialization;

namespace Raggle.Engines.Anthropic.ChatCompletion;

public class MessagesRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("messages")]
    public required ICollection<Message> Messages { get; set; }

    [JsonPropertyName("tool_choice")]
    public ToolChoice? ToolChoice { get; set; }

    [JsonPropertyName("tools")]
    public ICollection<Tool>? Tools { get; set; }

    [JsonPropertyName("max_tokens")]
    public required int MaxTokens { get; set; }

    [JsonPropertyName("system")]
    public string? System { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("stop_sequences")]
    public ICollection<string>? StopSequences { get; set; }

    [JsonPropertyName("metadata")]
    public object? Metadata { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }
}
