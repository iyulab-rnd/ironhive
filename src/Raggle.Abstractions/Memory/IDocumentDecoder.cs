﻿namespace Raggle.Abstractions.Memory;

public interface IDocumentDecoder
{
    /// <summary>
    /// Extract text content from the given file.
    /// </summary>
    Task<DocumentSource> DecodeAsync(
        DataPipeline pipeline,
        Stream data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if the specified content type is supported.
    /// </summary>
    /// <param name="mimeType">The MIME type of the content.</param>
    /// <returns>True if the content type is supported; otherwise, false.</returns>
    bool IsSupportMimeType(string mimeType);
}
