﻿using IronHive.Abstractions.ChatCompletion.Messages;
using IronHive.Abstractions.ChatCompletion.Tools;

namespace IronHive.Abstractions.ChatCompletion;

public class ChatCompletionRequest : ChatCompletionParameters
{
    /// <summary>
    /// chat completion model name.
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// system message to generate a response to.
    /// </summary>
    public string? System { get; set; }

    /// <summary>
    /// messages to generate a response to.
    /// </summary>
    public MessageCollection Messages { get; set; } = [];

    /// <summary>
    /// the tool list to use in the model.
    /// </summary>
    public FunctionToolCollection Tools { get; set; } = [];

}
