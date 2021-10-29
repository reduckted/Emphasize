using System.Collections.Generic;
using System.Linq;
using Xunit;


namespace Emphasize {

    public class EmphasisParserTests {


        [Fact]
        public void ReturnsEmptyCollectionForEmptyText() {
            EmphasisParser target;
            IEnumerable<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse("");

            Assert.Empty(results);
        }


        [Fact]
        public void ReturnsEmptyCollectionForWhitespace() {
            EmphasisParser target;
            IEnumerable<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse("    ");

            Assert.Empty(results);
        }


        [Fact]
        public void ReturnsEmptyCollectionWhenThereAreNoMarkers() {
            EmphasisParser target;
            IEnumerable<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse("this has no markers");

            Assert.Empty(results);
        }


        [Theory]
        [MemberData(nameof(GetMarkerTypes))]
        public void ReturnsSingleResultWhenMarkerCoversAllText(
                string marker,
                EmphasisType type
            ) {

            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse($"{marker}all covered{marker}").ToList();

            Assert.Equal(1, results.Count);
            Assert.Equal(0, results[0].StartOffset);
            Assert.Equal(13, results[0].Length);
            Assert.Equal(type, results[0].Type);
        }


        [Theory]
        [MemberData(nameof(GetMarkerTypes))]
        public void ReturnsSingleResultWhenMarkerIsAtStartOfText(
                string marker,
                EmphasisType type
            ) {

            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse($"{marker}this{marker} is marked").ToList();

            Assert.Equal(1, results.Count);
            Assert.Equal(0, results[0].StartOffset);
            Assert.Equal(6, results[0].Length);
            Assert.Equal(type, results[0].Type);
        }


        [Theory]
        [MemberData(nameof(GetMarkerTypes))]
        public void ReturnsSingleResultWhenMarkerIsInMiddleOfText(
                string marker,
                EmphasisType type
            ) {

            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse($"this {marker}is{marker} marked").ToList();

            Assert.Equal(1, results.Count);
            Assert.Equal(5, results[0].StartOffset);
            Assert.Equal(4, results[0].Length);
            Assert.Equal(type, results[0].Type);
        }


        [MemberData(nameof(GetMarkerTypes))]
        [Theory]
        public void ReturnsSingleResultWhenMarkerIsAtEndOfText(
                string marker,
                EmphasisType type
            ) {

            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse($"this is {marker}marked text{marker}").ToList();

            Assert.Equal(1, results.Count);
            Assert.Equal(8, results[0].StartOffset);
            Assert.Equal(13, results[0].Length);
            Assert.Equal(type, results[0].Type);
        }


        [MemberData(nameof(GetMarkerTypes))]
        [Theory]
        public void IgnoresMarkersInMiddleOfWords(
                string marker,
                EmphasisType type
            ) {

            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse($"mi{marker}dd{marker}le").ToList();

            Assert.Equal(0, results.Count);
        }


        [MemberData(nameof(GetMarkerTypesAndPunctuation))]
        [Theory]
        public void DetectsEndMarkerBeforePunctuation(
                string marker,
                EmphasisType type,
                char punctuation
            ) {

            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse($"this is {marker}marked{marker}{punctuation} and has punctuation").ToList();

            Assert.Equal(1, results.Count);
            Assert.Equal(8, results[0].StartOffset);
            Assert.Equal(8, results[0].Length);
            Assert.Equal(type, results[0].Type);
        }


        [MemberData(nameof(GetMarkerTypesAndOpeningBrackets))]
        [Theory]
        public void DetectsStartMarkerAfterBrackets(
                string marker,
                EmphasisType type,
                char bracket
            ) {

            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse($"this is {bracket}{marker}marked{marker} with brackets").ToList();

            Assert.Equal(1, results.Count);
            Assert.Equal(9, results[0].StartOffset);
            Assert.Equal(8, results[0].Length);
            Assert.Equal(type, results[0].Type);
        }


        [MemberData(nameof(GetMarkerTypesAndClosingBrackets))]
        [Theory]
        public void DetectsEndMarkerBeforeBrackets(
                string marker,
                EmphasisType type,
                char bracket
            ) {

            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse($"this is {marker}marked{marker}{bracket} with brackets").ToList();

            Assert.Equal(1, results.Count);
            Assert.Equal(8, results[0].StartOffset);
            Assert.Equal(8, results[0].Length);
            Assert.Equal(type, results[0].Type);
        }


        [MemberData(nameof(GetMarkerTypes))]
        [Theory]
        public void ReturnsMultipleResultsWhenTextContainsMultipleMarkedSpans(
                string marker,
                EmphasisType type
            ) {

            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse($"this {marker}is{marker} marked {marker}and{marker} so is this").ToList();

            Assert.Equal(2, results.Count);

            Assert.Equal(5, results[0].StartOffset);
            Assert.Equal(4, results[0].Length);
            Assert.Equal(type, results[0].Type);

            Assert.Equal(17, results[1].StartOffset);
            Assert.Equal(5, results[1].Length);
            Assert.Equal(type, results[1].Type);
        }


        [Fact]
        public void ReturnsMultipleResultsWhenTextContainsSpansOfDifferentTypes() {
            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse("this *is* marked _and_ so `is` this").ToList();

            Assert.Equal(3, results.Count);

            Assert.Equal(5, results[0].StartOffset);
            Assert.Equal(4, results[0].Length);
            Assert.Equal(EmphasisType.Bold, results[0].Type);

            Assert.Equal(17, results[1].StartOffset);
            Assert.Equal(5, results[1].Length);
            Assert.Equal(EmphasisType.Italic, results[1].Type);

            Assert.Equal(26, results[2].StartOffset);
            Assert.Equal(4, results[2].Length);
            Assert.Equal(EmphasisType.Code, results[2].Type);
        }


        [MemberData(nameof(GetMarkerTypes))]
        [Theory]
        public void IgnoresMarkersThatStartMidWord(
                string marker,
                string type
            ) {

            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse($"wo{marker}rd{marker}").ToList();

            Assert.Equal(0, results.Count);
        }


        [MemberData(nameof(GetMarkerTypes))]
        [Theory]
        public void IgnoresMarkersThatStartWithinWhitespace(
                string marker,
                string type
            ) {

            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse($"this {marker} is not{marker} marked").ToList();

            Assert.Equal(0, results.Count);
        }


        [MemberData(nameof(GetMarkerTypes))]
        [Theory]
        public void IgnoresMarkersThatEndWithinWhitespace(
                string marker,
                string type
            ) {

            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse($"this {marker}is not {marker} marked").ToList();

            Assert.Equal(0, results.Count);
        }


        [MemberData(nameof(GetMarkerTypes))]
        [Theory]
        public void IgnoresMarkersThatStartAndEndWithinWhitespace(
                string marker,
                string type
            ) {

            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse($"this {marker} is not {marker} marked").ToList();

            Assert.Equal(0, results.Count);
        }


        [MemberData(nameof(GetMarkerTypes))]
        [Theory]
        public void DoesNotProduceSpanWhenEndMarkerNotFound(
                string marker,
                string type
            ) {

            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse($"{marker}word").ToList();

            Assert.Equal(0, results.Count);
        }


        [Fact]
        public void ProducesSpansOfCombinedTypesWhenDifferentTypesAreNested() {
            EmphasisParser target;
            List<EmphasisSpan> results;
            int index;


            target = new EmphasisParser();
            results = target.Parse($"this has *_`mixed`_* types").ToList();

            Assert.Equal(5, results.Count);
            index = 0;

            Assert.Equal(9, results[index].StartOffset);
            Assert.Equal(1, results[index].Length);
            Assert.Equal(EmphasisType.Bold, results[index].Type);
            index += 1;

            Assert.Equal(10, results[index].StartOffset);
            Assert.Equal(1, results[index].Length);
            Assert.Equal(EmphasisType.Bold | EmphasisType.Italic, results[index].Type);
            index += 1;

            Assert.Equal(11, results[index].StartOffset);
            Assert.Equal(7, results[index].Length);
            Assert.Equal(EmphasisType.Bold | EmphasisType.Italic | EmphasisType.Code, results[index].Type);
            index += 1;

            Assert.Equal(18, results[index].StartOffset);
            Assert.Equal(1, results[index].Length);
            Assert.Equal(EmphasisType.Bold | EmphasisType.Italic, results[index].Type);
            index += 1;

            Assert.Equal(19, results[index].StartOffset);
            Assert.Equal(1, results[index].Length);
            Assert.Equal(EmphasisType.Bold, results[index].Type);
        }


        [Fact]
        public void ProducesSpansOfCombinedTypesWhenDifferentTypesOverlap() {
            EmphasisParser target;
            List<EmphasisSpan> results;
            int index;


            target = new EmphasisParser();
            results = target.Parse("this *has _mixed `span* types_ that *overlap` with* all _possible_ combinations").ToList();
            //                           bbbbbbbbbbbbbbbbbb             bbbbbbbbbbbbbbb
            //                                iiiiiiiiiiiiiiiiiiii                          iiiiiiiiii
            //                                       cccccccccccccccccccccccccccc

            Assert.Equal(8, results.Count);
            index = 0;

            Assert.Equal(5, results[index].StartOffset);
            Assert.Equal(5, results[index].Length);
            Assert.Equal(EmphasisType.Bold, results[index].Type);
            index += 1;

            Assert.Equal(10, results[index].StartOffset);
            Assert.Equal(7, results[index].Length);
            Assert.Equal(EmphasisType.Bold | EmphasisType.Italic, results[index].Type);
            index += 1;

            Assert.Equal(17, results[index].StartOffset);
            Assert.Equal(6, results[index].Length);
            Assert.Equal(EmphasisType.Bold | EmphasisType.Italic | EmphasisType.Code, results[index].Type);
            index += 1;

            Assert.Equal(23, results[index].StartOffset);
            Assert.Equal(7, results[index].Length);
            Assert.Equal(EmphasisType.Italic | EmphasisType.Code, results[index].Type);
            index += 1;

            Assert.Equal(30, results[index].StartOffset);
            Assert.Equal(6, results[index].Length);
            Assert.Equal(EmphasisType.Code, results[index].Type);
            index += 1;

            Assert.Equal(36, results[index].StartOffset);
            Assert.Equal(9, results[index].Length);
            Assert.Equal(EmphasisType.Bold | EmphasisType.Code, results[index].Type);
            index += 1;

            Assert.Equal(45, results[index].StartOffset);
            Assert.Equal(6, results[index].Length);
            Assert.Equal(EmphasisType.Bold, results[index].Type);
            index += 1;

            Assert.Equal(56, results[index].StartOffset);
            Assert.Equal(10, results[index].Length);
            Assert.Equal(EmphasisType.Italic, results[index].Type);
        }


        [Fact]
        public void IgnoresUnclosedSpansThatOverlapWithClosedSpans() {
            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse("this *has _unclosed spans*").ToList();

            Assert.Equal(1, results.Count);

            Assert.Equal(5, results[0].StartOffset);
            Assert.Equal(21, results[0].Length);
            Assert.Equal(EmphasisType.Bold, results[0].Type);
        }


        [MemberData(nameof(GetMarkerTypes))]
        [Theory]
        public void ReturnsCompletedSpansWhenThereAreIncompleteSpans(
                string marker,
                EmphasisType type
            ) {

            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse($"this {marker}comment{marker} is {marker}incomplete").ToList();

            Assert.Equal(1, results.Count);

            Assert.Equal(5, results[0].StartOffset);
            Assert.Equal(9, results[0].Length);
            Assert.Equal(type, results[0].Type);
        }


        [Fact]
        public void SupportsCStyleComments() {
            EmphasisParser target;
            List<EmphasisSpan> results;


            target = new EmphasisParser();
            results = target.Parse("/* this is a *c-style* comment */").ToList();

            Assert.Equal(1, results.Count);

            Assert.Equal(13, results[0].StartOffset);
            Assert.Equal(9, results[0].Length);
            Assert.Equal(EmphasisType.Bold, results[0].Type);
        }


        public static IEnumerable<object[]> GetMarkerTypes() {
            yield return new object[] { "*", EmphasisType.Bold };
            yield return new object[] { "_", EmphasisType.Italic };
            yield return new object[] { "`", EmphasisType.Code };
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


    }

}
