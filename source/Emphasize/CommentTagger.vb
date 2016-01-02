Imports Emphasize
Imports Microsoft.VisualStudio.Text
Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Text.Tagging


Public Class CommentTagger
    Implements ITagger(Of ClassificationTag)


    Private ReadOnly cgRegistry As IClassificationTypeRegistryService
    Private ReadOnly cgAggregator As ITagAggregator(Of IClassificationTag)
    Private ReadOnly cgParser As EmphasisParser


    Public Sub New(
            registry As IClassificationTypeRegistryService,
            aggregator As ITagAggregator(Of IClassificationTag),
            parser As EmphasisParser
        )

        cgRegistry = registry
        cgAggregator = aggregator
        cgParser = parser
    End Sub


    Public Event TagsChanged As EventHandler(Of SnapshotSpanEventArgs) _
        Implements ITagger(Of ClassificationTag).TagsChanged


    Public Function GetTags(spans As NormalizedSnapshotSpanCollection) As IEnumerable(Of ITagSpan(Of ClassificationTag)) _
        Implements ITagger(Of ClassificationTag).GetTags

        Return (
            From span In spans
            From mappingTag In cgAggregator.GetTags(span)
            Where IsCommentSpan(mappingTag)
            Let normalizedSnapshotSpans = mappingTag.Span.GetSpans(span.Snapshot)
            From tagSpan In GetTagSpansFromSnapshotSpans(normalizedSnapshotSpans)
            Select tagSpan
        ).ToList()
    End Function


    Private Iterator Function GetTagSpansFromSnapshotSpans(
            normalizedSnapshotSpans As NormalizedSnapshotSpanCollection
        ) As IEnumerable(Of ITagSpan(Of ClassificationTag))

        For Each span In normalizedSnapshotSpans
            Dim text As String


            text = span.Snapshot.GetText(span)

            If Not String.IsNullOrWhiteSpace(text) Then
                For Each item In cgParser.Parse(text)
                    Dim classification As IClassificationType


                    classification = GetClassificationForType(item.Type)

                    If classification IsNot Nothing Then
                        Dim tag As ClassificationTag
                        Dim tagSpan As SnapshotSpan


                        tag = New ClassificationTag(classification)
                        tagSpan = New SnapshotSpan(span.Snapshot, span.Start.Add(item.StartOffset), item.Length)

                        Yield New TagSpan(Of ClassificationTag)(tagSpan, tag)
                    End If
                Next item
            End If
        Next span
    End Function


    Private Function IsCommentSpan(mappingTag As IMappingTagSpan(Of IClassificationTag)) As Boolean
        Dim name As String


        name = mappingTag.Tag.ClassificationType.Classification

        Return name.IndexOf("comment", StringComparison.OrdinalIgnoreCase) >= 0
    End Function


    Private Function GetClassificationForType(type As EmphasisType) As IClassificationType
        Select Case type
            Case EmphasisType.Bold
                Return cgRegistry.GetClassificationType(FormatDefinitions.Bold.Name)

            Case EmphasisType.Italic
                Return cgRegistry.GetClassificationType(FormatDefinitions.Italic.Name)

            Case EmphasisType.Code
                Return cgRegistry.GetClassificationType(FormatDefinitions.Code.Name)

            Case EmphasisType.Bold Or EmphasisType.Italic
                Return cgRegistry.GetClassificationType(FormatDefinitions.BoldItalic.Name)

            Case EmphasisType.Bold Or EmphasisType.Code
                Return cgRegistry.GetClassificationType(FormatDefinitions.BoldCode.Name)

            Case EmphasisType.Italic Or EmphasisType.Code
                Return cgRegistry.GetClassificationType(FormatDefinitions.ItalicCode.Name)

            Case EmphasisType.Bold Or EmphasisType.Italic Or EmphasisType.Code
                Return cgRegistry.GetClassificationType(FormatDefinitions.BoldItalicCode.Name)

            Case Else
                Return Nothing

        End Select
    End Function

End Class
