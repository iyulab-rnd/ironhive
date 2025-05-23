﻿using System.Text.Json.Serialization;
using IronHive.Abstractions.Json;

namespace IronHive.Connectors.Anthropic.Models;

/// <summary>
/// Represents a Anthropic model.
/// </summary>
internal class AnthropicModel
{
    /// <summary>
    /// always "model"
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// the model ID.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// Display name of the model.
    /// </summary>
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Creation date and time of the model.
    /// </summary>
    [JsonPropertyName("created_at")]
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime? CreatedAt { get; set; }
}
