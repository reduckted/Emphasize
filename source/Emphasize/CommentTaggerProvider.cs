using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;


namespace Emphasize {

    [ContentType("code")]
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(ClassificationTag))]
    public class CommentTaggerProvider : IViewTaggerProvider {

        [Import]
        private IClassificationTypeRegistryService _classificationRegistry;


        [Import]
        private IBufferTagAggregatorFactoryService _aggregator;


        [Import]
        private EmphasisParser _parser;


        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag {
            return (ITagger<T>)new CommentTagger(
                _classificationRegistry,
                _aggregator.CreateTagAggregator<IClassificationTag>(buffer),
                _parser
            );
        }

    }

}
