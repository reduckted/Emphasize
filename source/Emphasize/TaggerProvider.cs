#nullable enable

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;


namespace Emphasize;

[ContentType("code")]
[Export(typeof(IViewTaggerProvider))]
[TagType(typeof(ClassificationTag))]
public class TaggerProvider : IViewTaggerProvider {

    private readonly IClassificationTypeRegistryService _classificationRegistry;
    private readonly IViewTagAggregatorFactoryService _aggregatorFactory;
    private readonly EmphasisParser _parser;
    private readonly OptionsProvider _options;
    private bool _creatingInnerTagAggregator;


    [ImportingConstructor]
    public TaggerProvider(
        IClassificationTypeRegistryService classificationRegistry,
        IViewTagAggregatorFactoryService aggregatorFactory,
        SVsServiceProvider serviceProvider,
        EmphasisParser parser
    ) {
        _classificationRegistry = classificationRegistry;
        _aggregatorFactory = aggregatorFactory;
        _parser = parser;
        _options = (OptionsProvider)ServiceProvider.GlobalProvider.GetService(typeof(OptionsProvider));
    }


    public ITagger<T>? CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag {
        if (_creatingInnerTagAggregator) {
            return null;
        }

        return textView.Properties.GetOrCreateSingletonProperty(typeof(Tagger), () => {
            ITagAggregator<IClassificationTag> tagAggregator;


            // To create the classification tag spans for Emphasize classifications,
            // we need to know where the comment spans are. The only way we can do that
            // is to use another classification tag aggregator. Creating that tag aggregator
            // will recursively call this `CreateTagger` method. Set a flag so that the
            // next call to this method should return null to avoid infinite recursion.
            _creatingInnerTagAggregator = true;

            try {
                tagAggregator = _aggregatorFactory.CreateTagAggregator<IClassificationTag>(textView);
            } finally {
                _creatingInnerTagAggregator = false;
            }

            return (ITagger<T>)new Tagger(_classificationRegistry, tagAggregator, _parser, _options);
        });
    }

}
