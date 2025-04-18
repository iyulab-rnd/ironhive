﻿using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EnabledThinking), "enabled")]
[JsonDerivedType(typeof(DisabledThinking), "disabled")]
internal interface IThinking
{ }

internal class EnabledThinking : IThinking
{
    [JsonPropertyName("budget_tokens")]
    public required int BudgetTokens { get; set; }
}

internal class DisabledThinking : IThinking
{ }
