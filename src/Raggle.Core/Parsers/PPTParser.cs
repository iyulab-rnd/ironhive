﻿using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using Raggle.Core.Document;
using Raggle.Core.Utils;

namespace Raggle.Core.Parsers;

public class PPTParser : IDocumentParser
{
    public string[] SupportContentTypes =>
    [
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
    ];

    public async Task<IEnumerable<DocumentSection>> ParseAsync(Stream data, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            // 취소 요청이 있는지 확인
            cancellationToken.ThrowIfCancellationRequested();

            var results = new List<DocumentSection>();

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
                var sectionText = string.Join(Environment.NewLine, slideText).TrimEnd();
                var cleanText = TextCleaner.Clean(sectionText);

                // Determine the slide number based on its position in the SlideIdList
                int slideNumber = slideIds.IndexOf(slideId) + 1;

                results.Add(new DocumentSection
                {
                    Identifier = $"Slide {slideNumber}",
                    Text = cleanText,
                });
            }

            // 최종적으로 한번 더 취소 요청 확인
            cancellationToken.ThrowIfCancellationRequested();

            return results;
        }, cancellationToken).ConfigureAwait(false);
    }
}
