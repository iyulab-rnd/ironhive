﻿using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;

namespace IronHive.Core.Memory.Decoders;

public class PPTDecoder : IDocumentDecoder
{
    /// <inheritdoc />
    public bool IsSupportMimeType(string mimeType)
    {
        return mimeType == "application/vnd.openxmlformats-officedocument.presentationml.presentation";
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> DecodeAsync(
        Stream data,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            // 취소 요청이 있는지 확인
            cancellationToken.ThrowIfCancellationRequested();

            var contents = new List<string>();

            using var presentation = PresentationDocument.Open(data, false);

            var presentationPart = presentation.PresentationPart
                ?? throw new InvalidOperationException("Presentation part is missing.");

            var slideIds = presentationPart?.Presentation.SlideIdList?.ChildElements.OfType<SlideId>().ToList()
                ?? throw new InvalidOperationException("Cannot find SlideIdList in the presentation.");

            foreach (var slideId in slideIds)
            {
                // Check for cancellation before processing each slide
                cancellationToken.ThrowIfCancellationRequested();

                // Get the relationship ID of the slide
                string? rId = slideId.RelationshipId
                    ?? throw new InvalidOperationException($"Relationship ID is missing for SlideId: {slideId}");

                // Get the corresponding SlidePart
                var slidePart = presentationPart.GetPartById(rId!) as SlidePart
                    ?? throw new InvalidOperationException($"SlidePart not found for relationship ID: {rId}");

                var slide = slidePart.Slide;

                // Extract all Text elements from the slide
                var slideText = slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>()
                                     .Select(t => t.Text)
                                     .ToList();

                // Combine the text and clean it
                var text = string.Join(Environment.NewLine, slideText).TrimEnd();
                text = TextCleaner.Clean(text);

                // Determine the slide number based on its position in the SlideIdList
                int slideNumber = slideIds.IndexOf(slideId) + 1;

                contents.Add(text);
            }

            cancellationToken.ThrowIfCancellationRequested();
            return contents;
        }, cancellationToken).ConfigureAwait(false);
    }
}
