﻿using System.Text.Json.Serialization;

namespace Raggle.Driver.OpenAI.ChatCompletion.Models;

internal class TokenUsage
{
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    [JsonPropertyName("completion_tokens_details")]
    public CompletionTokensDetails? CompletionTokensDetails { get; set; }

    [JsonPropertyName("prompt_tokens_details")]
    public PromptTokensDetails? PromptTokensDetails { get; set; }
}

internal class CompletionTokensDetails
{
    [JsonPropertyName("accepted_prediction_tokens")]
    public int AcceptedTokens { get; set; }

    [JsonPropertyName("audio_tokens")]
    public int AudioTokens { get; set; }

    [JsonPropertyName("reasoning_tokens")]
    public int ReasoningTokens { get; set; }

    [JsonPropertyName("rejected_prediction_tokens")]
    public int RejectedTokens { get; set; }
}

internal class PromptTokensDetails
{
    [JsonPropertyName("audio_tokens")]
    public int AudioTokens { get; set; }

    [JsonPropertyName("cached_tokens")]
    public int CachedTokens { get; set; }
}