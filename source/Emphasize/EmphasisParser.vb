Imports System.ComponentModel.Composition


<Export()>
Public Class EmphasisParser

    Private Shared ReadOnly OpeningBrackets As HashSet(Of Char)
    Private Shared ReadOnly ClosingBrackets As HashSet(Of Char)
    Private Shared ReadOnly Markers As Dictionary(Of Char, EmphasisType)


    Shared Sub New()
        OpeningBrackets = New HashSet(Of Char) From {"{"c, "["c, "<"c, "("c}
        ClosingBrackets = New HashSet(Of Char) From {"}"c, "]"c, ">"c, ")"c}

        Markers = New Dictionary(Of Char, EmphasisType)
        Markers.Add("*"c, EmphasisType.Bold)
        Markers.Add("_"c, EmphasisType.Italic)
        Markers.Add("`"c, EmphasisType.Code)
    End Sub


    Public Function Parse(text As String) As IEnumerable(Of EmphasisSpan)
        Dim spans As List(Of EmphasisSpan) = Nothing
        Dim typeStartIndexes As Dictionary(Of EmphasisType, Integer) = Nothing
        Dim currentSpanType As EmphasisType
        Dim startIndex As Integer


        startIndex = 0
        currentSpanType = EmphasisType.None

        For i = 0 To text.Length - 1
            Dim ch As Char
            Dim markerType As EmphasisType


            ch = text(i)

            ' Check if this character is a marker.
            If Markers.TryGetValue(ch, markerType) Then
                If (currentSpanType And markerType) = markerType Then
                    ' A span for this type is already open, so this marker
                    ' is possibly a closing marker. Check that it can be a
                    ' closing marker, and if it can be, end the current span.
                    If CanBeEndMarker(text, i, markerType, currentSpanType) Then
                        If spans Is Nothing Then
                            spans = New List(Of EmphasisSpan)
                        End If

                        ' Add a span that ends at the current index.
                        spans.Add(
                            New EmphasisSpan With {
                                .StartOffset = startIndex,
                                .Length = i - startIndex + 1,
                                .Type = currentSpanType
                            }
                        )

                        ' Remove the marker type from the new span, and record
                        ' that the new span starts from the next index.
                        currentSpanType = currentSpanType And (Not markerType)
                        startIndex = i + 1

                        ' Also remove the marker type from the tracking
                        ' collection of start indexes. This marker has closed,
                        ' so we don't need to remember where it starts.
                        typeStartIndexes.Remove(markerType)
                    End If

                Else
                    ' A span for this type is not already open, so this marker is
                    ' possibly an opening marker. Check that it can be an opening marker,
                    ' and if it can be, end the current span and start a new one.
                    If CanBeStartMarker(text, i, markerType, currentSpanType) Then
                        If currentSpanType <> EmphasisType.None Then
                            If spans Is Nothing Then
                                spans = New List(Of EmphasisSpan)
                            End If

                            ' Add a span that ends before the current index.
                            spans.Add(
                                New EmphasisSpan With {
                                    .StartOffset = startIndex,
                                    .Length = i - startIndex,
                                    .Type = currentSpanType
                                }
                            )
                        End If

                        currentSpanType = currentSpanType Or markerType
                        startIndex = i

                        ' Record where this marker type started so that we can remove
                        ' it from the other spans if it doesn't have a closing marker.
                        If typeStartIndexes Is Nothing Then
                            typeStartIndexes = New Dictionary(Of EmphasisType, Integer)
                        End If

                        typeStartIndexes(markerType) = i
                    End If
                End If
            End If
        Next i

        ' If there are any unclosed markers, we need to go
        ' back through the spans that we created and remove
        ' the unclosed emphasis type from those spans.
        If spans IsNot Nothing Then
            If currentSpanType <> EmphasisType.None Then
                Dim markerStartIndex As Integer


                If typeStartIndexes.TryGetValue(EmphasisType.Bold, markerStartIndex) Then
                    RemoveEmphasisFromLastSpans(EmphasisType.Bold, markerStartIndex, spans)
                End If

                If typeStartIndexes.TryGetValue(EmphasisType.Italic, markerStartIndex) Then
                    RemoveEmphasisFromLastSpans(EmphasisType.Italic, markerStartIndex, spans)
                End If

                If typeStartIndexes.TryGetValue(EmphasisType.Code, markerStartIndex) Then
                    RemoveEmphasisFromLastSpans(EmphasisType.Code, markerStartIndex, spans)
                End If

                ' Remove any spans from the end of the
                ' list that now have no emphasis type.
                For i = spans.Count - 1 To 0 Step -1
                    If spans(i).Type = EmphasisType.None Then
                        spans.RemoveAt(i)
                    Else
                        Exit For
                    End If
                Next i

                ' Because we have removed emphasis types from some of the
                ' spans, it's possible that some adjacent spans now have
                ' the same emphasis types. We need to merge these spans.
                MergeAdjacentSpans(spans)
            End If
        End If

        If spans IsNot Nothing Then
            Return spans
        Else
            Return Enumerable.Empty(Of EmphasisSpan)
        End If
    End Function


    Private Function CanBeStartMarker(
            text As String,
            charIndex As Integer,
            markerType As EmphasisType,
            spanType As EmphasisType
        ) As Boolean

        ' Markers cannot start at the end of the text, so there must
        ' be a following character, and it cannot be whitespace.
        If (charIndex < (text.Length - 1)) AndAlso (Not Char.IsWhiteSpace(text(charIndex + 1))) Then
            Dim previousChar As Char
            Dim previousType As EmphasisType


            ' Markers can start at the start of the text.
            If charIndex = 0 Then
                Return True
            End If

            previousChar = text(charIndex - 1)

            ' Markers can start after whitespace or an opening bracket.
            If Char.IsWhiteSpace(text(charIndex - 1)) OrElse OpeningBrackets.Contains(previousChar) Then
                Return True
            End If

            ' Or markers can start after another start marker.
            If Markers.TryGetValue(previousChar, previousType) Then
                ' The previous character would be a start marker if
                ' it's a different type to this marker, and that
                ' previous marker type is in the current span type.
                If previousType <> markerType Then
                    If (spanType And previousType) = previousType Then
                        Return True
                    End If
                End If
            End If
        End If

        Return False
    End Function


    Private Function CanBeEndMarker(
            text As String,
            charIndex As Integer,
            markerType As EmphasisType,
            spanType As EmphasisType
        ) As Boolean

        ' Markers can only end after a non-whitespace character.
        If (charIndex > 0) AndAlso (Not Char.IsWhiteSpace(text(charIndex - 1))) Then
            Dim nextChar As Char
            Dim nextType As EmphasisType


            ' Markers can end at the end of the text.
            If charIndex = (text.Length - 1) Then
                Return True
            End If

            nextChar = text(charIndex + 1)

            ' Markers can end before whitespace, punctuation, or a closing bracket.
            If Char.IsWhiteSpace(nextChar) OrElse Char.IsPunctuation(nextChar) OrElse ClosingBrackets.Contains(nextChar) Then
                Return True
            End If

            ' Or a marker can end before another end marker.
            If Markers.TryGetValue(nextChar, nextType) Then
                ' The next character will be an end marker if
                ' it's a different type to this marker, and that
                ' next marker type is in the current span type.
                If nextType <> markerType Then
                    If (spanType And nextType) = nextType Then
                        Return True
                    End If
                End If
            End If
        End If

        Return False
    End Function


    Private Sub RemoveEmphasisFromLastSpans(
            type As EmphasisType,
            unclosedMarkerStartIndex As Integer,
            spans As List(Of EmphasisSpan)
        )

        For i = spans.Count - 1 To 0 Step -1
            Dim temp As EmphasisSpan


            ' We can stop when we reach a span that ended before the unclosed
            ' marker started because it won't be affected by the unclosed marker.
            If (spans(i).StartOffset + spans(i).Length) < unclosedMarkerStartIndex Then
                Exit For
            End If

            ' This span included the unclosed marker, so
            ' we need to remove the type from this span.
            temp = spans(i)
            temp.Type = temp.Type And (Not type)
            spans(i) = temp
        Next i
    End Sub


    Private Sub MergeAdjacentSpans(spans As List(Of EmphasisSpan))
        For i = spans.Count - 1 To 1 Step -1
            Dim current As EmphasisSpan
            Dim earlier As EmphasisSpan


            current = spans(i)
            earlier = spans(i - 1)

            If current.StartOffset = (earlier.StartOffset + earlier.Length) Then
                If current.Type = earlier.Type Then
                    earlier.Length += current.Length
                    spans.RemoveAt(i)
                    spans(i - 1) = earlier
                End If
            End If
        Next i
    End Sub

End Class
