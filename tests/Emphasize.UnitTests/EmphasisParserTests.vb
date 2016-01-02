Public Class EmphasisParserTests

    Public Class ParseMethod

        <Fact()>
        Public Sub ReturnsEmptyCollectionForEmptyText()
            Dim target As EmphasisParser
            Dim results As IEnumerable(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse("")

            Assert.Empty(results)
        End Sub


        <Fact()>
        Public Sub ReturnsEmptyCollectionForWhitespace()
            Dim target As EmphasisParser
            Dim results As IEnumerable(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse("    ")

            Assert.Empty(results)
        End Sub


        <Fact()>
        Public Sub ReturnsEmptyCollectionWhenThereAreNoMarkers()
            Dim target As EmphasisParser
            Dim results As IEnumerable(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse("this has no markers")

            Assert.Empty(results)
        End Sub


        <MemberData(NameOf(GetMarkerTypes))>
        <Theory()>
        Public Sub ReturnsSingleResultWhenMarkerCoversAllText(
                marker As String,
                type As EmphasisType
            )

            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse($"{marker}all covered{marker}").ToList()

            Assert.Equal(1, results.Count)
            Assert.Equal(0, results(0).StartOffset)
            Assert.Equal(13, results(0).Length)
            Assert.Equal(type, results(0).Type)
        End Sub


        <MemberData(NameOf(GetMarkerTypes))>
        <Theory()>
        Public Sub ReturnsSingleResultWhenMarkerIsAtStartOfText(
                marker As String,
                type As EmphasisType
            )

            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse($"{marker}this{marker} is marked").ToList()

            Assert.Equal(1, results.Count)
            Assert.Equal(0, results(0).StartOffset)
            Assert.Equal(6, results(0).Length)
            Assert.Equal(type, results(0).Type)
        End Sub


        <MemberData(NameOf(GetMarkerTypes))>
        <Theory()>
        Public Sub ReturnsSingleResultWhenMarkerIsInMiddleOfText(
                marker As String,
                type As EmphasisType
            )

            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse($"this {marker}is{marker} marked").ToList()

            Assert.Equal(1, results.Count)
            Assert.Equal(5, results(0).StartOffset)
            Assert.Equal(4, results(0).Length)
            Assert.Equal(type, results(0).Type)
        End Sub


        <MemberData(NameOf(GetMarkerTypes))>
        <Theory()>
        Public Sub ReturnsSingleResultWhenMarkerIsAtEndOfText(
                marker As String,
                type As EmphasisType
            )

            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse($"this is {marker}marked text{marker}").ToList()

            Assert.Equal(1, results.Count)
            Assert.Equal(8, results(0).StartOffset)
            Assert.Equal(13, results(0).Length)
            Assert.Equal(type, results(0).Type)
        End Sub


        <MemberData(NameOf(GetMarkerTypes))>
        <Theory()>
        Public Sub IgnoresMarkersInMiddleOfWords(
                marker As String,
                type As EmphasisType
            )

            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse($"mi{marker}dd{marker}le").ToList()

            Assert.Equal(0, results.Count)
        End Sub


        <MemberData(NameOf(GetMarkerTypesAndPunctuation))>
        <Theory()>
        Public Sub DetectsEndMarkerBeforePunctuation(
                marker As String,
                type As EmphasisType,
                punctuation As Char
            )

            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse($"this is {marker}marked{marker}{punctuation} and has punctuation").ToList()

            Assert.Equal(1, results.Count)
            Assert.Equal(8, results(0).StartOffset)
            Assert.Equal(8, results(0).Length)
            Assert.Equal(type, results(0).Type)
        End Sub


        <MemberData(NameOf(GetMarkerTypesAndOpeningBrackets))>
        <Theory()>
        Public Sub DetectsStartMarkerAfterBrackets(
                marker As String,
                type As EmphasisType,
                bracket As Char
            )

            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse($"this is {bracket}{marker}marked{marker} with brackets").ToList()

            Assert.Equal(1, results.Count)
            Assert.Equal(9, results(0).StartOffset)
            Assert.Equal(8, results(0).Length)
            Assert.Equal(type, results(0).Type)
        End Sub


        <MemberData(NameOf(GetMarkerTypesAndClosingBrackets))>
        <Theory()>
        Public Sub DetectsEndMarkerBeforeBrackets(
                marker As String,
                type As EmphasisType,
                bracket As Char
            )

            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse($"this is {marker}marked{marker}{bracket} with brackets").ToList()

            Assert.Equal(1, results.Count)
            Assert.Equal(8, results(0).StartOffset)
            Assert.Equal(8, results(0).Length)
            Assert.Equal(type, results(0).Type)
        End Sub


        <MemberData(NameOf(GetMarkerTypes))>
        <Theory()>
        Public Sub ReturnsMultipleResultsWhenTextContainsMultipleMarkedSpans(
                marker As String,
                type As EmphasisType
            )

            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse($"this {marker}is{marker} marked {marker}and{marker} so is this").ToList()

            Assert.Equal(2, results.Count)

            Assert.Equal(5, results(0).StartOffset)
            Assert.Equal(4, results(0).Length)
            Assert.Equal(type, results(0).Type)

            Assert.Equal(17, results(1).StartOffset)
            Assert.Equal(5, results(1).Length)
            Assert.Equal(type, results(1).Type)
        End Sub


        <Fact()>
        Public Sub ReturnsMultipleResultsWhenTextContainsSpansOfDifferentTypes()
            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse("this *is* marked _and_ so `is` this").ToList()

            Assert.Equal(3, results.Count)

            Assert.Equal(5, results(0).StartOffset)
            Assert.Equal(4, results(0).Length)
            Assert.Equal(EmphasisType.Bold, results(0).Type)

            Assert.Equal(17, results(1).StartOffset)
            Assert.Equal(5, results(1).Length)
            Assert.Equal(EmphasisType.Italic, results(1).Type)

            Assert.Equal(26, results(2).StartOffset)
            Assert.Equal(4, results(2).Length)
            Assert.Equal(EmphasisType.Code, results(2).Type)
        End Sub


        <MemberData(NameOf(GetMarkerTypes))>
        <Theory()>
        Public Sub IgnoresMarkersThatStartMidWord(
                marker As String,
                type As String
            )

            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse($"wo{marker}rd{marker}").ToList()

            Assert.Equal(0, results.Count)
        End Sub


        <MemberData(NameOf(GetMarkerTypes))>
        <Theory()>
        Public Sub IgnoresMarkersThatStartWithinWhitespace(
                marker As String,
                type As String
            )

            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse($"this {marker} is not{marker} marked").ToList()

            Assert.Equal(0, results.Count)
        End Sub


        <MemberData(NameOf(GetMarkerTypes))>
        <Theory()>
        Public Sub IgnoresMarkersThatEndWithinWhitespace(
                marker As String,
                type As String
            )

            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse($"this {marker}is not {marker} marked").ToList()

            Assert.Equal(0, results.Count)
        End Sub


        <MemberData(NameOf(GetMarkerTypes))>
        <Theory()>
        Public Sub IgnoresMarkersThatStartAndEndWithinWhitespace(
                marker As String,
                type As String
            )

            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse($"this {marker} is not {marker} marked").ToList()

            Assert.Equal(0, results.Count)
        End Sub


        <MemberData(NameOf(GetMarkerTypes))>
        <Theory()>
        Public Sub DoesNotProduceSpanWhenEndMarkerNotFound(
                marker As String,
                type As String
            )

            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse($"{marker}word").ToList()

            Assert.Equal(0, results.Count)
        End Sub


        <Fact()>
        Public Sub ProducesSpansOfCombinedTypesWhenDifferentTypesAreNested()
            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)
            Dim index As Integer


            target = New EmphasisParser
            results = target.Parse($"this has *_`mixed`_* types").ToList()

            Assert.Equal(5, results.Count)

            Assert.Equal(9, results(index).StartOffset)
            Assert.Equal(1, results(index).Length)
            Assert.Equal(EmphasisType.Bold, results(index).Type)
            index += 1

            Assert.Equal(10, results(index).StartOffset)
            Assert.Equal(1, results(index).Length)
            Assert.Equal(EmphasisType.Bold Or EmphasisType.Italic, results(index).Type)
            index += 1

            Assert.Equal(11, results(index).StartOffset)
            Assert.Equal(7, results(index).Length)
            Assert.Equal(EmphasisType.Bold Or EmphasisType.Italic Or EmphasisType.Code, results(index).Type)
            index += 1

            Assert.Equal(18, results(index).StartOffset)
            Assert.Equal(1, results(index).Length)
            Assert.Equal(EmphasisType.Bold Or EmphasisType.Italic, results(index).Type)
            index += 1

            Assert.Equal(19, results(index).StartOffset)
            Assert.Equal(1, results(index).Length)
            Assert.Equal(EmphasisType.Bold, results(index).Type)
        End Sub


        <Fact()>
        Public Sub ProducesSpansOfCombinedTypesWhenDifferentTypesOverlap()
            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)
            Dim index As Integer


            target = New EmphasisParser
            results = target.Parse("this *has _mixed `span* types_ that *overlap` with* all _possible_ combinations").ToList()
            '                            bbbbbbbbbbbbbbbbbb             bbbbbbbbbbbbbbb
            '                                 iiiiiiiiiiiiiiiiiiii                          iiiiiiiiii
            '                                        cccccccccccccccccccccccccccc

            Assert.Equal(8, results.Count)

            Assert.Equal(5, results(index).StartOffset)
            Assert.Equal(5, results(index).Length)
            Assert.Equal(EmphasisType.Bold, results(index).Type)
            index += 1

            Assert.Equal(10, results(index).StartOffset)
            Assert.Equal(7, results(index).Length)
            Assert.Equal(EmphasisType.Bold Or EmphasisType.Italic, results(index).Type)
            index += 1

            Assert.Equal(17, results(index).StartOffset)
            Assert.Equal(6, results(index).Length)
            Assert.Equal(EmphasisType.Bold Or EmphasisType.Italic Or EmphasisType.Code, results(index).Type)
            index += 1

            Assert.Equal(23, results(index).StartOffset)
            Assert.Equal(7, results(index).Length)
            Assert.Equal(EmphasisType.Italic Or EmphasisType.Code, results(index).Type)
            index += 1

            Assert.Equal(30, results(index).StartOffset)
            Assert.Equal(6, results(index).Length)
            Assert.Equal(EmphasisType.Code, results(index).Type)
            index += 1

            Assert.Equal(36, results(index).StartOffset)
            Assert.Equal(9, results(index).Length)
            Assert.Equal(EmphasisType.Bold Or EmphasisType.Code, results(index).Type)
            index += 1

            Assert.Equal(45, results(index).StartOffset)
            Assert.Equal(6, results(index).Length)
            Assert.Equal(EmphasisType.Bold, results(index).Type)
            index += 1

            Assert.Equal(56, results(index).StartOffset)
            Assert.Equal(10, results(index).Length)
            Assert.Equal(EmphasisType.Italic, results(index).Type)
        End Sub


        <Fact()>
        Public Sub IgnoresUnclosedSpansThatOverlapWithClosedSpans()
            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse("this *has _unclosed spans*").ToList()

            Assert.Equal(1, results.Count)

            Assert.Equal(5, results(0).StartOffset)
            Assert.Equal(21, results(0).Length)
            Assert.Equal(EmphasisType.Bold, results(0).Type)
        End Sub


        <MemberData(NameOf(GetMarkerTypes))>
        <Theory()>
        Public Sub ReturnsCompletedSpansWhenThereAreIncompleteSpans(
                marker As String,
                type As EmphasisType
            )

            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse($"this {marker}comment{marker} is {marker}incomplete").ToList()

            Assert.Equal(1, results.Count)

            Assert.Equal(5, results(0).StartOffset)
            Assert.Equal(9, results(0).Length)
            Assert.Equal(type, results(0).Type)
        End Sub


        <Fact()>
        Public Sub SupportsCStyleComments()
            Dim target As EmphasisParser
            Dim results As List(Of EmphasisSpan)


            target = New EmphasisParser
            results = target.Parse("/* this is a *c-style* comment */").ToList()

            Assert.Equal(1, results.Count)

            Assert.Equal(13, results(0).StartOffset)
            Assert.Equal(9, results(0).Length)
            Assert.Equal(EmphasisType.Bold, results(0).Type)
        End Sub


        Public Shared Iterator Function GetMarkerTypes() As IEnumerable(Of Object())
            Yield {"*", EmphasisType.Bold}
            Yield {"_", EmphasisType.Italic}
            Yield {"`", EmphasisType.Code}
        End Function


        Public Shared Iterator Function GetMarkerTypesAndPunctuation() As IEnumerable(Of Object())
            For Each item In GetMarkerTypes()
                For Each ch In {"."c, ","c, "?"c, "!"c, ":"c, ";"c}
                    Yield item.Concat(New Object() {ch}).ToArray()
                Next ch
            Next item
        End Function


        Public Shared Iterator Function GetMarkerTypesAndOpeningBrackets() As IEnumerable(Of Object())
            For Each item In GetMarkerTypes()
                For Each ch In {"{"c, "<"c, "("c, "["c}
                    Yield item.Concat(New Object() {ch}).ToArray()
                Next ch
            Next item
        End Function


        Public Shared Iterator Function GetMarkerTypesAndClosingBrackets() As IEnumerable(Of Object())
            For Each item In GetMarkerTypes()
                For Each ch In {"}"c, ">"c, ")"c, "]"c}
                    Yield item.Concat(New Object() {ch}).ToArray()
                Next ch
            Next item
        End Function

    End Class

End Class
