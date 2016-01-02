Imports Microsoft.VisualStudio.Text
Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Text.Editor
Imports Microsoft.VisualStudio.Text.Tagging
Imports Microsoft.VisualStudio.Utilities
Imports System.ComponentModel.Composition


<ContentType("code")>
<Export(GetType(IViewTaggerProvider))>
<TagType(GetType(ClassificationTag))>
Friend Class CommentTaggerProvider
    Implements IViewTaggerProvider


    <Import()>
    Private cgClassificationRegistry As IClassificationTypeRegistryService


    <Import()>
    Private cgAggregator As IBufferTagAggregatorFactoryService


    <Import()>
    Private cgParser As EmphasisParser


    Public Function CreateTagger(Of T As ITag)(
            textView As ITextView,
            buffer As ITextBuffer
        ) As ITagger(Of T) _
        Implements IViewTaggerProvider.CreateTagger

        Dim tagger As CommentTagger


        tagger = New CommentTagger(
            cgClassificationRegistry,
            cgAggregator.CreateTagAggregator(Of IClassificationTag)(buffer),
            cgParser
        )

        Return DirectCast(tagger, ITagger(Of T))
    End Function

End Class
