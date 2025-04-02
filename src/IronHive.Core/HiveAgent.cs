﻿using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;

namespace IronHive.Core;

public class HiveAgent : IHiveAgent
{
    public required string Provider { get; set; }

    public required string Model { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public string? Instructions { get; set; }

    public IDictionary<string, object>? Tools { get; set; }

    public ChatCompletionParameters? Parameters { get; set; }
}
