using System.Collections.Generic;
using System.Linq;
using Xunit;


namespace Emphasize {

    public class EmphasisParserTests {

        [Fact]
        public void ReturnsEmptyCollectionForEmptyText() {
            Assert.Empty(Parse(""));
        }


        [Fact]
        public void ReturnsEmptyCollectionForWhitespace() {
            Assert.Empty(Parse("    "));
        }


        [Fact]
        public void ReturnsEmptyCollectionWhenThereAreNoMarkers() {
            Assert.Empty(Parse("this has no markers"));
        }


        [Theory]
        [MemberData(nameof(GetMarkerTypes))]
        public void ReturnsSingleResultWhenMarkerCoversAllText(string marker, EmphasisType type) {
            IEnumerable<EmphasisSpan> results;


            results = Parse($"{marker}all covered{marker}");

            Assert.Equal(
                new[] { new EmphasisSpan { StartOffset = 0, Length = 13, Type = type } },
                results
            );
        }


        [Theory]
        [MemberData(nameof(GetMarkerTypes))]
        public void ReturnsSingleResultWhenMarkerIsAtStartOfText(string marker, EmphasisType type) {
            IEnumerable<EmphasisSpan> results;


            results = Parse($"{marker}this{marker} is marked");

            Assert.Equal(
                new[] { new EmphasisSpan { StartOffset = 0, Length = 6, Type = type } },
                results
            );
        }


        [Theory]
        [MemberData(nameof(GetMarkerTypes))]
        public void ReturnsSingleResultWhenMarkerIsInMiddleOfText(string marker, EmphasisType type) {
            IEnumerable<EmphasisSpan> results;


            results = Parse($"this {marker}is{marker} marked");

            Assert.Equal(
                new[] { new EmphasisSpan { StartOffset = 5, Length = 4, Type = type } },
                results
            );
        }


        [Theory]
        [MemberData(nameof(GetMarkerTypes))]
        public void ReturnsSingleResultWhenMarkerIsAtEndOfText(string marker, EmphasisType type) {
            IEnumerable<EmphasisSpan> results;


            results = Parse($"this is {marker}marked text{marker}");

            Assert.Equal(
                new[] { new EmphasisSpan { StartOffset = 8, Length = 13, Type = type } },
                results
            );
        }


        [Theory]
        [MemberData(nameof(GetMarkers))]
        public void IgnoresMarkersInMiddleOfWords(string marker) {
            Assert.Empty(Parse($"mi{marker}dd{marker}le"));
        }


        [Theory]
        [MemberData(nameof(GetMarkerTypesAndPunctuation))]
        public void DetectsEndMarkerBeforePunctuation(string marker, EmphasisType type, char punctuation) {
            IEnumerable<EmphasisSpan> results;


            results = Parse($"this is {marker}marked{marker}{punctuation} and has punctuation");

            Assert.Equal(
                new[] { new EmphasisSpan { StartOffset = 8, Length = 8, Type = type } },
                results
            );
        }


        [Theory]
        [MemberData(nameof(GetMarkerTypesAndOpeningBrackets))]
        public void DetectsStartMarkerAfterBrackets(string marker, EmphasisType type, char bracket) {
            IEnumerable<EmphasisSpan> results;


            results = Parse($"this is {bracket}{marker}marked{marker} with brackets");

            Assert.Equal(
                new[] { new EmphasisSpan { StartOffset = 9, Length = 8, Type = type } },
                results
            );
        }


        [Theory]
        [MemberData(nameof(GetMarkerTypesAndClosingBrackets))]
        public void DetectsEndMarkerBeforeBrackets(string marker, EmphasisType type, char bracket) {
            IEnumerable<EmphasisSpan> results;


            results = Parse($"this is {marker}marked{marker}{bracket} with brackets");

            Assert.Equal(
                new[] { new EmphasisSpan { StartOffset = 8, Length = 8, Type = type } },
                results
            );
        }


        [Theory]
        [MemberData(nameof(GetMarkerTypes))]
        public void ReturnsMultipleResultsWhenTextContainsMultipleMarkedSpans(string marker, EmphasisType type) {
            IEnumerable<EmphasisSpan> results;


            results = Parse($"this {marker}is{marker} marked {marker}and{marker} so is this");

            Assert.Equal(
                new[] {
                    new  EmphasisSpan {StartOffset = 5, Length = 4, Type = type },
                    new  EmphasisSpan {StartOffset = 17, Length = 5, Type = type }
                },
                results
            );
        }


        [Fact]
        public void ReturnsMultipleResultsWhenTextContainsSpansOfDifferentTypes() {
            IEnumerable<EmphasisSpan> results;


            results = Parse("this *is* marked _and_ so `is` this");

            Assert.Equal(
                new[] {
                    new EmphasisSpan{ StartOffset = 5, Length = 4, Type = EmphasisType.Bold },
                    new EmphasisSpan{ StartOffset = 17, Length = 5, Type = EmphasisType.Italic },
                    new EmphasisSpan{ StartOffset = 26, Length = 4, Type = EmphasisType.Code }
                },
                results
            );
        }


        [Theory]
        [MemberData(nameof(GetMarkers))]
        public void IgnoresMarkersThatStartMidWord(string marker) {
            Assert.Empty(Parse($"wo{marker}rd{marker}"));
        }


        [Theory]
        [MemberData(nameof(GetMarkers))]
        public void IgnoresMarkersThatStartWithinWhitespace(string marker) {
            Assert.Empty(Parse($"this {marker} is not{marker} marked"));
        }


        [Theory]
        [MemberData(nameof(GetMarkers))]
        public void IgnoresMarkersThatEndWithinWhitespace(string marker) {
            Assert.Empty(Parse($"this {marker}is not {marker} marked"));
        }


        [Theory]
        [MemberData(nameof(GetMarkers))]
        public void IgnoresMarkersThatStartAndEndWithinWhitespace(string marker) {
            Assert.Empty(Parse($"this {marker} is not {marker} marked"));
        }

        [Theory]
        [MemberData(nameof(GetMarkers))]
        public void DoesNotProduceSpanWhenEndMarkerNotFound(string marker) {
            Assert.Empty(Parse($"{marker}word"));
        }


        [Fact]
        public void ProducesSpansOfCombinedTypesWhenDifferentTypesAreNested() {
            IEnumerable<EmphasisSpan> results;


            results = Parse($"this has *_`mixed`_* types");

            Assert.Equal(
                new[] {
                    new EmphasisSpan { StartOffset = 9, Length = 1, Type = EmphasisType.Bold },
                    new EmphasisSpan { StartOffset = 10, Length = 1, Type = EmphasisType.Bold | EmphasisType.Italic },
                    new EmphasisSpan { StartOffset = 11, Length = 7, Type = EmphasisType.Bold | EmphasisType.Italic | EmphasisType.Code },
                    new EmphasisSpan { StartOffset = 18, Length = 1, Type = EmphasisType.Bold | EmphasisType.Italic },
                    new EmphasisSpan { StartOffset = 19, Length = 1, Type = EmphasisType.Bold }
                },
                results
            );
        }


        [Fact]
        public void ProducesSpansOfCombinedTypesWhenDifferentTypesOverlap() {
            IEnumerable<EmphasisSpan> results;


            results = Parse("this *has _mixed `span* types_ that *overlap` with* all _possible_ combinations");
            //                    bbbbbbbbbbbbbbbbbb             bbbbbbbbbbbbbbb
            //                         iiiiiiiiiiiiiiiiiiii                          iiiiiiiiii
            //                                cccccccccccccccccccccccccccc

            Assert.Equal(
                new[] {
                    new EmphasisSpan { StartOffset = 5, Length = 5, Type = EmphasisType.Bold },
                    new EmphasisSpan { StartOffset = 10, Length = 7, Type = EmphasisType.Bold | EmphasisType.Italic },
                    new EmphasisSpan { StartOffset = 17, Length = 6, Type = EmphasisType.Bold | EmphasisType.Italic | EmphasisType.Code },
                    new EmphasisSpan { StartOffset = 23, Length = 7, Type = EmphasisType.Italic | EmphasisType.Code },
                    new EmphasisSpan { StartOffset = 30, Length = 6, Type = EmphasisType.Code },
                    new EmphasisSpan { StartOffset = 36, Length = 9, Type = EmphasisType.Bold | EmphasisType.Code },
                    new EmphasisSpan { StartOffset = 45, Length = 6, Type = EmphasisType.Bold },
                    new EmphasisSpan { StartOffset = 56, Length = 10, Type = EmphasisType.Italic }
                },
                results
            );
        }


        [Fact]
        public void IgnoresUnclosedSpansThatOverlapWithClosedSpans() {
            IEnumerable<EmphasisSpan> results;


            results = Parse("this *has _unclosed spans*");

            Assert.Equal(
                new[] { new EmphasisSpan { StartOffset = 5, Length = 21, Type = EmphasisType.Bold } },
                results
            );
        }


        [Theory]
        [MemberData(nameof(GetMarkerTypes))]
        public void ReturnsCompletedSpansWhenThereAreIncompleteSpans(string marker, EmphasisType type) {
            IEnumerable<EmphasisSpan> results;


            results = Parse($"this {marker}comment{marker} is {marker}incomplete");

            Assert.Equal(
                new[] { new EmphasisSpan { StartOffset = 5, Length = 9, Type = type } },
                results
            );
        }


        [Fact]
        public void SupportsCStyleComments() {
            IEnumerable<EmphasisSpan> results;


            results = Parse("/* this is a *c-style* comment */");

            Assert.Equal(
                new[] { new EmphasisSpan { StartOffset = 13, Length = 9, Type = EmphasisType.Bold } },
                results
            );
        }


        public static IEnumerable<object[]> GetMarkerTypes() {
            yield return new object[] { "*", EmphasisType.Bold };
            yield return new object[] { "_", EmphasisType.Italic };
            yield return new object[] { "`", EmphasisType.Code };
        }


        public static IEnumerable<object[]> GetMarkers() {
            foreach (var item in GetMarkerTypes()) {
                yield return new object[] { item[0] };
            }
        }


        public static IEnumerable<object[]> GetMarkerTypesAndPunctuation() {
            foreach (var item in GetMarkerTypes()) {
                foreach (char ch in new[] { '.', ',', '?', '!', ':', ';' }) {
                    yield return item.Concat(new object[] { ch }).ToArray();
                }
            }
        }


        public static IEnumerable<object[]> GetMarkerTypesAndOpeningBrackets() {
            foreach (var item in GetMarkerTypes()) {
                foreach (char ch in new[] { '{', '<', '(', '[' }) {
                    yield return item.Concat(new object[] { ch }).ToArray();
                }
            }
        }


        public static IEnumerable<object[]> GetMarkerTypesAndClosingBrackets() {
            foreach (var item in GetMarkerTypes()) {
                foreach (char ch in new[] { '}', '>', ')', ']' }) {
                    yield return item.Concat(new object[] { ch }).ToArray();
                }
            }
        }


        private static IEnumerable<EmphasisSpan> Parse(string text) {
            return new EmphasisParser().Parse(text);
        }

    }

}
