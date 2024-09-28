using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emphasize {

    public class Tagger : ITagger<ClassificationTag> {

        private readonly IClassificationTypeRegistryService _registry;
        private readonly EmphasisParser _parser;


        public Tagger(IClassificationTypeRegistryService registry, EmphasisParser parser) {
            _registry = registry;
            _parser = parser;
        }


        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;


        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans) {
            return (
                from span in spans
                from tagSpan in GetTagSpansFromSnapshotSpan(span)
                select tagSpan
            ).ToList();
        }


        private IEnumerable<ITagSpan<ClassificationTag>> GetTagSpansFromSnapshotSpan(SnapshotSpan span) {
            string text;


            text = span.GetText();

            if (!string.IsNullOrWhiteSpace(text)) {
                foreach (EmphasisSpan item in _parser.Parse(text)) {
                    IClassificationType classification;


                    classification = GetClassificationForType(item.Type);

                    if (classification != null) {
                        yield return new TagSpan<ClassificationTag>(
                            new SnapshotSpan(span.Snapshot, span.Start.Add(item.StartOffset), item.Length),
                            new ClassificationTag(classification)
                        );
                    }
                }
            }
        }


        private IClassificationType GetClassificationForType(EmphasisType type) {
            switch (type) {
                case EmphasisType.Bold:
                    return _registry.GetClassificationType(FormatDefinitions.Bold.Name);

                case EmphasisType.Italic:
                    return _registry.GetClassificationType(FormatDefinitions.Italic.Name);

                case EmphasisType.Code:
                    return _registry.GetClassificationType(FormatDefinitions.Code.Name);

                case EmphasisType.Bold | EmphasisType.Italic:
                    return _registry.GetClassificationType(FormatDefinitions.BoldItalic.Name);

                case EmphasisType.Bold | EmphasisType.Code:
                    return _registry.GetClassificationType(FormatDefinitions.BoldCode.Name);

                case EmphasisType.Italic | EmphasisType.Code:
                    return _registry.GetClassificationType(FormatDefinitions.ItalicCode.Name);

                case EmphasisType.Bold | EmphasisType.Italic | EmphasisType.Code:
                    return _registry.GetClassificationType(FormatDefinitions.BoldItalicCode.Name);

                default:
                    return null;

            }

        }

    }

}
