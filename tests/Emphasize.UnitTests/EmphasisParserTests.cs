using System.Collections.Generic;
using System.Linq;
using Xunit;


namespace Emphasize;

public class EmphasisParserTests {

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReturnsEmptyCollectionForEmptyText(bool useMarkdownStyle) {
        Assert.Empty(Parse("", useMarkdownStyle));
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReturnsEmptyCollectionForWhitespace(bool useMarkdownStyle) {
        Assert.Empty(Parse("    ", useMarkdownStyle));
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReturnsEmptyCollectionWhenThereAreNoMarkers(bool useMarkdownStyle) {
        Assert.Empty(Parse("this has no markers", useMarkdownStyle));
    }


    [Theory]
    [MemberData(nameof(GetMarkerTypes))]
    public void ReturnsSingleResultWhenMarkerCoversAllText(string marker, EmphasisType type, bool useMarkdownStyle) {
        IEnumerable<EmphasisSpan> results;


        results = Parse($"{marker}all covered{marker}", useMarkdownStyle);

        Assert.Equal(
            [new EmphasisSpan { StartOffset = 0, Length = 11 + (2 * marker.Length), Type = type }],
            results
        );
    }


    [Theory]
    [MemberData(nameof(GetMarkerTypes))]
    public void ReturnsSingleResultWhenMarkerIsAtStartOfText(string marker, EmphasisType type, bool useMarkdownStyle) {
        IEnumerable<EmphasisSpan> results;


        results = Parse($"{marker}this{marker} is marked", useMarkdownStyle);

        Assert.Equal(
            [new EmphasisSpan { StartOffset = 0, Length = 4 + (2 * marker.Length), Type = type }],
            results
        );
    }


    [Theory]
    [MemberData(nameof(GetMarkerTypes))]
    public void ReturnsSingleResultWhenMarkerIsInMiddleOfText(string marker, EmphasisType type, bool useMarkdownStyle) {
        IEnumerable<EmphasisSpan> results;


        results = Parse($"this {marker}is{marker} marked", useMarkdownStyle);

        Assert.Equal(
            [new EmphasisSpan { StartOffset = 5, Length = 2 + (2 * marker.Length), Type = type }],
            results
        );
    }


    [Theory]
    [MemberData(nameof(GetMarkerTypes))]
    public void ReturnsSingleResultWhenMarkerIsAtEndOfText(string marker, EmphasisType type, bool useMarkdownStyle) {
        IEnumerable<EmphasisSpan> results;


        results = Parse($"this is {marker}marked text{marker}", useMarkdownStyle);

        Assert.Equal(
            [new EmphasisSpan { StartOffset = 8, Length = 11 + (2 * marker.Length), Type = type }],
            results
        );
    }


    [Theory]
    [MemberData(nameof(GetMarkers))]
    public void IgnoresMarkersInMiddleOfWords(string marker, bool useMarkdownStyle) {
        Assert.Empty(Parse($"mi{marker}dd{marker}le", useMarkdownStyle));
    }


    [Theory]
    [MemberData(nameof(GetMarkerTypesAndPunctuation))]
    public void DetectsEndMarkerBeforePunctuation(string marker, EmphasisType type, bool useMarkdownStyle, char punctuation) {
        IEnumerable<EmphasisSpan> results;


        results = Parse($"this is {marker}marked{marker}{punctuation} and has punctuation", useMarkdownStyle);

        Assert.Equal(
            [new EmphasisSpan { StartOffset = 8, Length = 6 + (2 * marker.Length), Type = type }],
            results
        );
    }


    [Theory]
    [MemberData(nameof(GetMarkerTypesAndOpeningBrackets))]
    public void DetectsStartMarkerAfterBrackets(string marker, EmphasisType type, bool useMarkdownStyle, char bracket) {
        IEnumerable<EmphasisSpan> results;


        results = Parse($"this is {bracket}{marker}marked{marker} with brackets", useMarkdownStyle);

        Assert.Equal(
            [new EmphasisSpan { StartOffset = 9, Length = 6 + (2 * marker.Length), Type = type }],
            results
        );
    }


    [Theory]
    [MemberData(nameof(GetMarkerTypesAndClosingBrackets))]
    public void DetectsEndMarkerBeforeBrackets(string marker, EmphasisType type, bool useMarkdownStyle, char bracket) {
        IEnumerable<EmphasisSpan> results;


        results = Parse($"this is {marker}marked{marker}{bracket} with brackets", useMarkdownStyle);

        Assert.Equal(
            [new EmphasisSpan { StartOffset = 8, Length = 6 + (2 * marker.Length), Type = type }],
            results
        );
    }


    [Theory]
    [MemberData(nameof(GetMarkerTypes))]
    public void ReturnsMultipleResultsWhenTextContainsMultipleMarkedSpans(string marker, EmphasisType type, bool useMarkdownStyle) {
        IEnumerable<EmphasisSpan> results;


        results = Parse($"this {marker}is{marker} marked {marker}and{marker} so is this", useMarkdownStyle);

        Assert.Equal(
            [
                new EmphasisSpan {StartOffset = 5, Length = 2 + (2 * marker.Length), Type = type },
                new EmphasisSpan {StartOffset = 15 + (2 * marker.Length), Length = 3 + (2 * marker.Length), Type = type }
            ],
            results
        );
    }


    [Fact]
    public void ReturnsMultipleResultsWhenTextContainsSpansOfDifferentTypesAndUseMarkdownStyleIsFalse() {
        IEnumerable<EmphasisSpan> results;


        results = Parse("this *is* marked _and_ so `is` this", false);

        Assert.Equal(
            [
                new EmphasisSpan{ StartOffset = 5, Length = 4, Type = EmphasisType.Bold },
                new EmphasisSpan{ StartOffset = 17, Length = 5, Type = EmphasisType.Italic },
                new EmphasisSpan{ StartOffset = 26, Length = 4, Type = EmphasisType.Code }
            ],
            results
        );
    }


    [Fact]
    public void ReturnsMultipleResultsWhenTextContainsSpansOfDifferentTypesAndUseMarkdownStyleIsTrue() {
        IEnumerable<EmphasisSpan> results;


        results = Parse("this *is* marked _and_ so `is` this and **so is** this", true);

        Assert.Equal(
            [
                new EmphasisSpan{ StartOffset = 5, Length = 4, Type = EmphasisType.Italic },
                new EmphasisSpan{ StartOffset = 17, Length = 5, Type = EmphasisType.Italic },
                new EmphasisSpan{ StartOffset = 26, Length = 4, Type = EmphasisType.Code },
                new EmphasisSpan{ StartOffset = 40, Length = 9, Type = EmphasisType.Bold}
            ],
            results
        );
    }


    [Theory]
    [MemberData(nameof(GetMarkers))]
    public void IgnoresMarkersThatStartMidWord(string marker, bool useMarkdownStyle) {
        Assert.Empty(Parse($"wo{marker}rd{marker}", useMarkdownStyle));
    }


    [Theory]
    [MemberData(nameof(GetMarkers))]
    public void IgnoresMarkersThatStartWithinWhitespace(string marker, bool useMarkdownStyle) {
        Assert.Empty(Parse($"this {marker} is not{marker} marked", useMarkdownStyle));
    }


    [Theory]
    [MemberData(nameof(GetMarkers))]
    public void IgnoresMarkersThatEndWithinWhitespace(string marker, bool useMarkdownStyle) {
        Assert.Empty(Parse($"this {marker}is not {marker} marked", useMarkdownStyle));
    }


    [Theory]
    [MemberData(nameof(GetMarkers))]
    public void IgnoresMarkersThatStartAndEndWithinWhitespace(string marker, bool useMarkdownStyle) {
        Assert.Empty(Parse($"this {marker} is not {marker} marked", useMarkdownStyle));
    }

    [Theory]
    [MemberData(nameof(GetMarkers))]
    public void DoesNotProduceSpanWhenEndMarkerNotFound(string marker, bool useMarkdownStyle) {
        Assert.Empty(Parse($"{marker}word", useMarkdownStyle));
    }


    [Fact]
    public void ProducesSpansOfCombinedTypesWhenDifferentTypesAreNestedAndUseMarkdownStyleIsFalse() {
        IEnumerable<EmphasisSpan> results;


        results = Parse($"this has *_`mixed`_* types", false);

        Assert.Equal(
            [
                new EmphasisSpan { StartOffset = 9, Length = 1, Type = EmphasisType.Bold },
                new EmphasisSpan { StartOffset = 10, Length = 1, Type = EmphasisType.Bold | EmphasisType.Italic },
                new EmphasisSpan { StartOffset = 11, Length = 7, Type = EmphasisType.Bold | EmphasisType.Italic | EmphasisType.Code },
                new EmphasisSpan { StartOffset = 18, Length = 1, Type = EmphasisType.Bold | EmphasisType.Italic },
                new EmphasisSpan { StartOffset = 19, Length = 1, Type = EmphasisType.Bold }
            ],
            results
        );
    }


    [Fact]
    public void ProducesSpansOfCombinedTypesWhenDifferentTypesAreNestedAndUseMarkdownStyleIsTrue() {
        IEnumerable<EmphasisSpan> results;


        results = Parse($"this has **_`mixed`_** types", true);

        Assert.Equal(
            [
                new EmphasisSpan { StartOffset = 9, Length = 2, Type = EmphasisType.Bold },
                new EmphasisSpan { StartOffset = 11, Length = 1, Type = EmphasisType.Bold | EmphasisType.Italic },
                new EmphasisSpan { StartOffset = 12, Length = 7, Type = EmphasisType.Bold | EmphasisType.Italic | EmphasisType.Code },
                new EmphasisSpan { StartOffset = 19, Length = 1, Type = EmphasisType.Bold | EmphasisType.Italic },
                new EmphasisSpan { StartOffset = 20, Length = 2, Type = EmphasisType.Bold }
            ],
            results
        );
    }


    [Fact]
    public void ProducesSpansOfCombinedTypesWhenDifferentTypesOverlapAndUseMarkdownStyleIsFalse() {
        IEnumerable<EmphasisSpan> results;


        results = Parse("this *has _mixed `span* types_ that *overlap` with* all _possible_ combinations", false);
        //                    bbbbbbbbbbbbbbbbbb             bbbbbbbbbbbbbbb
        //                         iiiiiiiiiiiiiiiiiiii                          iiiiiiiiii
        //                                cccccccccccccccccccccccccccc

        Assert.Equal(
            [
                new EmphasisSpan { StartOffset = 5, Length = 5, Type = EmphasisType.Bold },
                new EmphasisSpan { StartOffset = 10, Length = 7, Type = EmphasisType.Bold | EmphasisType.Italic },
                new EmphasisSpan { StartOffset = 17, Length = 6, Type = EmphasisType.Bold | EmphasisType.Italic | EmphasisType.Code },
                new EmphasisSpan { StartOffset = 23, Length = 7, Type = EmphasisType.Italic | EmphasisType.Code },
                new EmphasisSpan { StartOffset = 30, Length = 6, Type = EmphasisType.Code },
                new EmphasisSpan { StartOffset = 36, Length = 9, Type = EmphasisType.Bold | EmphasisType.Code },
                new EmphasisSpan { StartOffset = 45, Length = 6, Type = EmphasisType.Bold },
                new EmphasisSpan { StartOffset = 56, Length = 10, Type = EmphasisType.Italic }
            ],
            results
        );
    }


    [Fact]
    public void ProducesSpansOfCombinedTypesWhenDifferentTypesOverlapAndUseMarkdownStyleIsTrue() {
        IEnumerable<EmphasisSpan> results;


        results = Parse("this **has _mixed `span** types_ that **overlap` with** all _possible_ combinations", true);
        //                    bbbbbbbbbbbbbbbbbbbb             bbbbbbbbbbbbbbbbb
        //                          iiiiiiiiiiiiiiiiiiiii                            iiiiiiiiii
        //                                 cccccccccccccccccccccccccccccc

        Assert.Equal(
            [
                new EmphasisSpan { StartOffset = 5, Length = 6, Type = EmphasisType.Bold },
                new EmphasisSpan { StartOffset = 11, Length = 7, Type = EmphasisType.Bold | EmphasisType.Italic },
                new EmphasisSpan { StartOffset = 18, Length = 7, Type = EmphasisType.Bold | EmphasisType.Italic | EmphasisType.Code },
                new EmphasisSpan { StartOffset = 25, Length = 7, Type = EmphasisType.Italic | EmphasisType.Code },
                new EmphasisSpan { StartOffset = 32, Length = 6, Type = EmphasisType.Code },
                new EmphasisSpan { StartOffset = 38, Length = 10, Type = EmphasisType.Bold | EmphasisType.Code },
                new EmphasisSpan { StartOffset = 48, Length = 7, Type = EmphasisType.Bold },
                new EmphasisSpan { StartOffset = 60, Length = 10, Type = EmphasisType.Italic }
            ],
            results
        );
    }


    [Theory]
    [InlineData("*", false)]
    [InlineData("**", true)]
    public void IgnoresUnclosedSpansThatOverlapWithClosedSpans(string marker, bool useMarkdownStyle) {
        IEnumerable<EmphasisSpan> results;


        results = Parse($"this {marker}has _unclosed spans{marker}", useMarkdownStyle);

        Assert.Equal(
            [new EmphasisSpan { StartOffset = 5, Length = 19 + (2 * marker.Length), Type = EmphasisType.Bold }],
            results
        );
    }


    [Theory]
    [MemberData(nameof(GetMarkerTypes))]
    public void ReturnsCompletedSpansWhenThereAreIncompleteSpans(string marker, EmphasisType type, bool useMarkdownStyle) {
        IEnumerable<EmphasisSpan> results;


        results = Parse($"this {marker}comment{marker} is {marker}incomplete", useMarkdownStyle);

        Assert.Equal(
            [new EmphasisSpan { StartOffset = 5, Length = 7 + (2 * marker.Length), Type = type }],
            results
        );
    }


    [Theory]
    [InlineData("*", false)]
    [InlineData("**", true)]
    public void SupportsCStyleComments(string marker, bool useMarkdownStyle) {
        IEnumerable<EmphasisSpan> results;


        results = Parse($"/* this is a {marker}c-style{marker} comment */", useMarkdownStyle);

        Assert.Equal(
            [new EmphasisSpan { StartOffset = 13, Length = 7 + (2 * marker.Length), Type = EmphasisType.Bold }],
            results
        );
    }


    public static IEnumerable<TheoryDataRow<string, EmphasisType, bool>> GetMarkerTypes() {
        yield return new("*", EmphasisType.Bold, false);
        yield return new("_", EmphasisType.Italic, false);
        yield return new("`", EmphasisType.Code, false);

        yield return new("*", EmphasisType.Italic, true);
        yield return new("**", EmphasisType.Bold, true);
        yield return new("_", EmphasisType.Italic, true);
        yield return new("`", EmphasisType.Code, true);
    }


    public static TheoryData<string, bool> GetMarkers() {
        return [.. GetMarkerTypes().Select((x) => (x.Data.Item1, x.Data.Item3))];
    }


    public static IEnumerable<TheoryDataRow<string, EmphasisType, bool, char>> GetMarkerTypesAndPunctuation() {
        foreach (var item in GetMarkerTypes()) {
            foreach (char ch in new[] { '.', ',', '?', '!', ':', ';' }) {
                yield return new(item.Data.Item1, item.Data.Item2, item.Data.Item3, ch);
            }
        }
    }


    public static IEnumerable<TheoryDataRow<string, EmphasisType, bool, char>> GetMarkerTypesAndOpeningBrackets() {
        foreach (var item in GetMarkerTypes()) {
            foreach (char ch in new[] { '{', '<', '(', '[' }) {
                yield return new(item.Data.Item1, item.Data.Item2, item.Data.Item3, ch);
            }
        }
    }


    public static IEnumerable<TheoryDataRow<string, EmphasisType, bool, char>> GetMarkerTypesAndClosingBrackets() {
        foreach (var item in GetMarkerTypes()) {
            foreach (char ch in new[] { '}', '>', ')', ']' }) {
                yield return new(item.Data.Item1, item.Data.Item2, item.Data.Item3, ch);
            }
        }
    }


    private static IEnumerable<EmphasisSpan> Parse(string text, bool useMarkdownStyle) {
        return new EmphasisParser().Parse(text, useMarkdownStyle);
    }

}
