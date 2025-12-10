#nullable enable

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Emphasize; 

[Export]
public class EmphasisParser {

    private static readonly HashSet<char> OpeningBrackets = ['{', '[', '<', '('];
    private static readonly HashSet<char> ClosingBrackets = ['}', ']', '>', ')'];
    private static readonly Dictionary<char, EmphasisType> Markers = new() {
        {'*', EmphasisType.Bold},
        {'_', EmphasisType.Italic},
        {'`', EmphasisType.Code }
    };


    public IEnumerable<EmphasisSpan> Parse(string text) {
        List<EmphasisSpan>? spans = null;
        Dictionary<EmphasisType, int>? typeStartIndexes = null;
        EmphasisType currentSpanType;
        int startIndex;


        startIndex = 0;
        currentSpanType = EmphasisType.None;

        for (int i = 0; i < text.Length; i++) {
            char ch;


            ch = text[i];

            // Check if this character is a marker.
            if (Markers.TryGetValue(ch, out EmphasisType markerType)) {
                if ((currentSpanType & markerType) == markerType) {
                    // A span for this type is already open, so this marker
                    // is possibly a closing marker. Check that it can be a
                    // closing marker, and if it can be, end the current span.
                    if (CanBeEndMarker(text, i, markerType, currentSpanType)) {
                        spans ??= [];

                        // Add a span that ends at the current index.
                        spans.Add(
                            new EmphasisSpan {
                                StartOffset = startIndex,
                                Length = i - startIndex + 1,
                                Type = currentSpanType
                            }
                        );

                        // Remove the marker type from the new span, and record
                        // that the new span starts from the next index.
                        currentSpanType &= (~markerType);
                        startIndex = i + 1;

                        // Also remove the marker type from the tracking
                        // collection of start indexes. This marker has closed,
                        // so we don't need to remember where it starts.
                        typeStartIndexes?.Remove(markerType);
                    }

                } else {
                    // A span for this type is not already open, so this marker is
                    // possibly an opening marker. Check that it can be an opening marker,
                    // and if it can be, end the current span and start a new one.
                    if (CanBeStartMarker(text, i, markerType, currentSpanType)) {
                        if (currentSpanType != EmphasisType.None) {
                            spans ??= [];

                            // Add a span that ends before the current index.
                            spans.Add(
                                new EmphasisSpan {
                                    StartOffset = startIndex,
                                    Length = i - startIndex,
                                    Type = currentSpanType
                                }
                            );
                        }

                        currentSpanType |= markerType;
                        startIndex = i;

                        // Record where this marker type started so that we can remove
                        // it from the other spans if it doesn't have a closing marker.
                        typeStartIndexes ??= [];
                        typeStartIndexes[markerType] = i;
                    }
                }
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


    private bool CanBeStartMarker(string text, int charIndex, EmphasisType markerType, EmphasisType spanType) {
        // Markers cannot start at the end of the text, so there must
        // be a following character, and it cannot be whitespace.
        if ((charIndex < (text.Length - 1)) && (!char.IsWhiteSpace(text[charIndex + 1]))) {
            char previousChar;


            // Markers can start at the start of the text.
            if (charIndex == 0) {
                return true;
            }

            previousChar = text[charIndex - 1];

            // Markers can start after whitespace or an opening bracket.
            if (char.IsWhiteSpace(text[charIndex - 1]) || OpeningBrackets.Contains(previousChar)) {
                return true;
            }

            // Or markers can start after another start marker.
            if (Markers.TryGetValue(previousChar, out EmphasisType previousType)) {
                // The previous character would be a start marker if
                // it's a different type to this marker, and that
                // previous marker type is in the current span type.
                if (previousType != markerType) {
                    if ((spanType & previousType) == previousType) {
                        return true;
                    }
                }
            }
        }

        return false;
    }


    private bool CanBeEndMarker(string text, int charIndex, EmphasisType markerType, EmphasisType spanType) {
        // Markers can only end after a non-whitespace character.
        if ((charIndex > 0) && (!char.IsWhiteSpace(text[charIndex - 1]))) {
            char nextChar;


            // Markers can end at the end of the text.
            if (charIndex == (text.Length - 1)) {
                return true;
            }

            nextChar = text[charIndex + 1];

            // Markers can end before whitespace, punctuation, or a closing bracket.
            if (char.IsWhiteSpace(nextChar) || char.IsPunctuation(nextChar) || ClosingBrackets.Contains(nextChar)) {
                return true;
            }

            // Or a marker can end before another end marker.
            if (Markers.TryGetValue(nextChar, out EmphasisType nextType)) {
                // The next character will be an end marker if
                // it's a different type to this marker, and that
                // next marker type is in the current span type.
                if (nextType != markerType) {
                    if ((spanType & nextType) == nextType) {
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

}
