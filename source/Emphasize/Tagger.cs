#nullable enable

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emphasize {

    public class Tagger : ITagger<ClassificationTag> {

        private readonly IClassificationTypeRegistryService _registry;
        private readonly ITagAggregator<IClassificationTag> _aggregator;
        private readonly EmphasisParser _parser;


        public Tagger(
            IClassificationTypeRegistryService registry,
            ITagAggregator<IClassificationTag> aggregator,
            EmphasisParser parser
        ) {
            _registry = registry;
            _aggregator = aggregator;
            _parser = parser;
        }


        public event EventHandler<SnapshotSpanEventArgs>? TagsChanged {
            add { }
            remove { }
        }


        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans) {
            return (
                from span in spans
                from mappingTag in _aggregator.GetTags(spans)
                where IsCommentSpan(mappingTag)
                from mappingSpan in mappingTag.Span.GetSpans(mappingTag.Span.AnchorBuffer.CurrentSnapshot)
                from tagSpan in GetTagSpans(mappingSpan)
                select tagSpan
            ).ToList();
        }


        private static bool IsCommentSpan(IMappingTagSpan<IClassificationTag> mappingTag) {
            return mappingTag
                .Tag
                .ClassificationType
                .Classification
                .IndexOf("comment", StringComparison.OrdinalIgnoreCase) >= 0;
        }


        private IEnumerable<ITagSpan<ClassificationTag>> GetTagSpans(SnapshotSpan span) {
            string text;


            text = span.GetText();

            if (!string.IsNullOrWhiteSpace(text)) {
                foreach (EmphasisSpan item in _parser.Parse(text)) {
                    IClassificationType? classification;


                    classification = GetClassificationForType(item.Type);

                    if (classification is not null) {
                        yield return new TagSpan<ClassificationTag>(
                            new SnapshotSpan(span.Snapshot, span.Start.Add(item.StartOffset), item.Length),
                            new ClassificationTag(classification)
                        );
                    }
                }
            }
        }


        private IClassificationType? GetClassificationForType(EmphasisType type) {
            return (type) switch {
                EmphasisType.Bold => _registry.GetClassificationType(FormatDefinitions.Bold.Name),
                EmphasisType.Italic => _registry.GetClassificationType(FormatDefinitions.Italic.Name),
                EmphasisType.Code => _registry.GetClassificationType(FormatDefinitions.Code.Name),
                EmphasisType.Bold | EmphasisType.Italic => _registry.GetClassificationType(FormatDefinitions.BoldItalic.Name),
                EmphasisType.Bold | EmphasisType.Code => _registry.GetClassificationType(FormatDefinitions.BoldCode.Name),
                EmphasisType.Italic | EmphasisType.Code => _registry.GetClassificationType(FormatDefinitions.ItalicCode.Name),
                EmphasisType.Bold | EmphasisType.Italic | EmphasisType.Code => _registry.GetClassificationType(FormatDefinitions.BoldItalicCode.Name),
                _ => null
            };
        }

    }

}
