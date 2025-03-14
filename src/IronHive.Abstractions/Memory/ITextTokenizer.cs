﻿namespace IronHive.Abstractions.Memory;

public interface ITextTokenizer
{
    /// <summary>
    /// Count the number of tokens contained in the given text.
    /// </summary>
    /// <param name="text">Text to analyze</param>
    /// <returns>Number of tokens</returns>
    public int CountTokens(string text);

    /// <summary>
    /// Return token values
    /// </summary>
    /// <param name="text">Text to parse</param>
    /// <returns>Collection of token value</returns>
    IReadOnlyList<int> Encode(string text);

    /// <summary>
    /// Return token strings
    /// </summary>
    /// <param name="tokens">Token values</param>
    /// <returns>Decoded string value</returns>
    string Decode(IReadOnlyList<int> tokens);
}
