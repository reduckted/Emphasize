#nullable enable

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Emphasize;

[Export]
public class EmphasisParser {

    private static readonly HashSet<char> OpeningBrackets = ['{', '[', '<', '('];
    private static readonly HashSet<char> ClosingBrackets = ['}', ']', '>', ')'];


    public IEnumerable<EmphasisSpan> Parse(string text, bool useMarkdownStyle) {
        List<EmphasisSpan>? spans = null;
        Dictionary<EmphasisType, int>? typeStartIndexes = null;
        EmphasisType currentSpanType;
        int startIndex;
        int charIndex;
        Marker? previousStartMarker;


        startIndex = 0;
        charIndex = 0;
        currentSpanType = EmphasisType.None;
        previousStartMarker = null;

        while (charIndex < text.Length) {
            // Check if this character could be a marker.
            if (TryGetMarker(text, charIndex, useMarkdownStyle, out Marker marker)) {
                if ((currentSpanType & marker.Type) == marker.Type) {
                    // A span for this type is already open, so this marker
                    // is possibly a closing marker. Check that it can be a
                    // closing marker, and if it can be, end the current span.
                    if (CanBeEndMarker(text, charIndex, useMarkdownStyle, marker, currentSpanType)) {
                        spans ??= [];

                        // Add a span that ends at the current index.
                        spans.Add(
                            new EmphasisSpan {
                                StartOffset = startIndex,
                                Length = charIndex - startIndex + marker.Length,
                                Type = currentSpanType
                            }
                        );

                        // Remove the marker type from the new span, and record
                        // that the new span starts from the next index.
                        currentSpanType &= (~marker.Type);
                        startIndex = charIndex + marker.Length;

                        // Also remove the marker type from the tracking
                        // collection of start indexes. This marker has closed,
                        // so we don't need to remember where it starts.
                        typeStartIndexes?.Remove(marker.Type);

                        // Skip past the marker.
                        charIndex += marker.Length;

                    } else {
                        // The marker that we found can't actually be a
                        // marker, so we can just move to the next character.
                        charIndex += 1;
                    }

                    previousStartMarker = null;

                } else {
                    // A span for this type is not already open, so this marker is
                    // possibly an opening marker. Check that it can be an opening marker,
                    // and if it can be, end the current span and start a new one.
                    if (CanBeStartMarker(text, charIndex, marker, previousStartMarker)) {
                        if (currentSpanType != EmphasisType.None) {
                            spans ??= [];

                            // Add a span that ends before the current index.
                            spans.Add(
                                new EmphasisSpan {
                                    StartOffset = startIndex,
                                    Length = charIndex - startIndex,
                                    Type = currentSpanType
                                }
                            );
                        }

                        currentSpanType |= marker.Type;
                        startIndex = charIndex;

                        // Record where this marker type started so that we can remove
                        // it from the other spans if it doesn't have a closing marker.
                        typeStartIndexes ??= [];
                        typeStartIndexes[marker.Type] = charIndex;

                        // Skip past the marker.
                        charIndex += marker.Length;
                        previousStartMarker = marker;

                    } else {
                        // The marker that we found can't actually be a
                        // marker, so we can just move to the next character.
                        charIndex += 1;
                        previousStartMarker = null;
                    }
                }

            } else {
                charIndex += 1;
                previousStartMarker = null;
            }
        }

        // If there are any unclosed markers, we need to go
        // back through the spans that we created and remove
        // the unclosed emphasis type from those spans.
        if (spans is not null) {
            if ((currentSpanType != EmphasisType.None) && (typeStartIndexes is not null)) {
                if (typeStartIndexes.TryGetValue(EmphasisType.Bold, out int markerStartIndex)) {
                    RemoveEmphasisFromLastSpans(EmphasisType.Bold, markerStartIndex, spans);
                }

                if (typeStartIndexes.TryGetValue(EmphasisType.Italic, out markerStartIndex)) {
                    RemoveEmphasisFromLastSpans(EmphasisType.Italic, markerStartIndex, spans);
                }

                if (typeStartIndexes.TryGetValue(EmphasisType.Code, out markerStartIndex)) {
                    RemoveEmphasisFromLastSpans(EmphasisType.Code, markerStartIndex, spans);
                }

                // Remove any spans from the end of the
                // list that now have no emphasis type.
                for (int i = spans.Count - 1; i >= 0; i--) {
                    if (spans[i].Type == EmphasisType.None) {
                        spans.RemoveAt(i);
                    } else {
                        break;
                    }
                }

                // Because we have removed emphasis types from some of the
                // spans, it's possible that some adjacent spans now have
                // the same emphasis types. We need to merge these spans.
                MergeAdjacentSpans(spans);
            }
        }

        return spans ?? Enumerable.Empty<EmphasisSpan>();
    }


    private static bool TryGetMarker(
        string text,
        int charIndex,
        bool useMarkdownStyle,
        out Marker marker
    ) {
        char ch;


        ch = text[charIndex];

        if (ch == '_') {
            marker = new Marker(EmphasisType.Italic, 1);
            return true;
        }

        if (ch == '`') {
            marker = new Marker(EmphasisType.Code, 1);
            return true;
        }

        if (ch == '*') {
            if (useMarkdownStyle) {
                if (((charIndex + 1) < text.Length) && (text[charIndex + 1] == '*')) {
                    marker = new Marker(EmphasisType.Bold, 2);
                } else {
                    marker = new Marker(EmphasisType.Italic, 1);
                }

                return true;

            } else {
                marker = new Marker(EmphasisType.Bold, 1);
                return true;
            }
        }

        marker = default;
        return false;
    }


    private bool CanBeStartMarker(string text, int charIndex, Marker currentMarker, Marker? previousStartMarker) {
        // Markers cannot start at the end of the text, so there must
        // be a following character, and it cannot be whitespace.
        if (((charIndex + currentMarker.Length) < text.Length) && (!char.IsWhiteSpace(text[charIndex + currentMarker.Length]))) {
            char previousChar;


            // Markers can start at the start of the text.
            if (charIndex == 0) {
                return true;
            }

            previousChar = text[charIndex - 1];

            // Markers can start after whitespace or an opening bracket.
            if (char.IsWhiteSpace(previousChar) || OpeningBrackets.Contains(previousChar)) {
                return true;
            }

            // Or markers can start after another start marker.
            if (previousStartMarker.HasValue) {
                return true;
            }
        }

        return false;
    }


    private bool CanBeEndMarker(
        string text,
        int charIndex,
        bool useMarkdownStyle,
        Marker currentMarker,
        EmphasisType spanType
    ) {
        // Markers can only end after a non-whitespace character.
        if ((charIndex > 0) && (!char.IsWhiteSpace(text[charIndex - 1]))) {
            char nextChar;


            // Markers can end at the end of the text.
            if ((charIndex + currentMarker.Length) == text.Length) {
                return true;
            }

            nextChar = text[charIndex + currentMarker.Length];

            // Markers can end before whitespace, punctuation, or a closing bracket.
            if (char.IsWhiteSpace(nextChar) || char.IsPunctuation(nextChar) || ClosingBrackets.Contains(nextChar)) {
                return true;
            }

            // Or a marker can end before another end marker.
            if (TryGetMarker(text, charIndex + currentMarker.Length, useMarkdownStyle, out Marker nextMarker)) {
                // The next marker will be an end marker if
                // it's a different type to this marker, and that
                // next marker type is in the current span type.
                if (nextMarker.Type != currentMarker.Type) {
                    if ((spanType & nextMarker.Type) == nextMarker.Type) {
                        return true;
                    }
                }
            }
        }

        return false;
    }


    private void RemoveEmphasisFromLastSpans(EmphasisType type, int unclosedMarkerStartIndex, List<EmphasisSpan> spans) {
        for (int i = spans.Count - 1; i >= 0; i--) {
            EmphasisSpan temp;


            // We can stop when we reach a span that ended before the unclosed
            // marker started because it won't be affected by the unclosed marker.
            if ((spans[i].StartOffset + spans[i].Length) < unclosedMarkerStartIndex) {
                break;
            }

            // This span included the unclosed marker, so
            // we need to remove the type from this span.
            temp = spans[i];
            temp.Type &= (~type);
            spans[i] = temp;
        }
    }


    private void MergeAdjacentSpans(List<EmphasisSpan> spans) {
        for (int i = spans.Count - 1; i >= 1; i--) {
            EmphasisSpan current;
            EmphasisSpan earlier;


            current = spans[i];
            earlier = spans[i - 1];

            if (current.StartOffset == (earlier.StartOffset + earlier.Length)) {
                if (current.Type == earlier.Type) {
                    earlier.Length += current.Length;
                    spans.RemoveAt(i);
                    spans[i - 1] = earlier;
                }
            }
        }
    }



    private record struct Marker(EmphasisType Type, int Length);

}
