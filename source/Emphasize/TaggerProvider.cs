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
    public class TaggerProvider : IViewTaggerProvider {

        private static readonly object TaggerProperty = new object();


        private readonly IClassificationTypeRegistryService _classificationRegistry;
        private readonly EmphasisParser _parser;


        [ImportingConstructor]
        public TaggerProvider(
            IClassificationTypeRegistryService classificationRegistry,
            EmphasisParser parser
        ) {
            _classificationRegistry = classificationRegistry;
            _parser = parser;
        }


        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag {
            return textView.Properties.GetOrCreateSingletonProperty(
                TaggerProperty,
                () => (ITagger<T>)new Tagger(_classificationRegistry, _parser)
            );
        }

    }

}
