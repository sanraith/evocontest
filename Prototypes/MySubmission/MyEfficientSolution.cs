using evocontest.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MySubmission
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }

    public sealed class MyEfficientSolution : ISolution
    {
        [DebuggerDisplay("{Acronym}, {WordIndexes.Length}")]
        private class ProcessedInput
        {
            public string Acronym { get; set; }

            public (int Index, int Length, bool IsNormalWordOrEndOfAcronym)[] WordIndexes { get; set; }

            public int[] PeriodIndexes { get; set; }
        }

        public sealed class Replacement
        {
            public int Index;
            public int Length;
            public List<int> Occurences;
        }

        public string Solve(string input)
        {
            if (input == string.Empty) { return input; }

            var processedInput = GetSentences(input);
            var replacements = GetReplacements(input, processedInput);
            var result = ReplaceAcronyms(input, processedInput, replacements);

            return result;
        }

        private static string ReplaceAcronyms(string input, ProcessedInput processedInput, Dictionary<int, Replacement> replacements)
        {
            var inputLength = input.Length;
            var targetSpan = new char[inputLength].AsSpan();

            var targetIndex = 0;
            var acronymSpan = processedInput.Acronym.AsSpan();
            for (int inputIndex = 0; inputIndex < inputLength; inputIndex++)
            {
                if (replacements.TryGetValue(inputIndex, out var replacement))
                {
                    acronymSpan[replacement.Index..(replacement.Index + replacement.Length)].CopyTo(targetSpan[targetIndex..]);
                    var lastWordIndex = processedInput.WordIndexes[replacement.Index + replacement.Length - 1];
                    inputIndex = lastWordIndex.Index + lastWordIndex.Length - 1;
                    targetIndex += replacement.Length;
                }
                else
                {
                    targetSpan[targetIndex] = input[inputIndex];
                    targetIndex++;
                }
            }

            return new string(targetSpan[..targetIndex]);
        }

        private Dictionary<int, Replacement> GetReplacements(string input, ProcessedInput processedInput)
        {
            var inputSpan = input.AsSpan();
            var wordIndexes = processedInput.WordIndexes;
            List<Replacement> replacements = new List<Replacement>();
            var upperText = processedInput.Acronym;
            var minLength = 2;
            var maxLength = 30; // TODO
            HashSet<int> allOccurences = new HashSet<int>();
            for (int i = 0; i < upperText.Length; i++)
            {
                //TODO more efficient
                if (allOccurences.Contains(i) ||
                    (i > 0 && !wordIndexes[i - 1].IsNormalWordOrEndOfAcronym))
                {
                    continue;
                }

                List<int> occurences = null;
                var possibleReplacements = new List<Replacement>();
                for (var aLength = minLength; aLength <= maxLength && aLength <= upperText.Length - i; aLength++)
                {
                    if (!wordIndexes[i + aLength - 1].IsNormalWordOrEndOfAcronym)
                    {
                        continue;
                    }

                    // TODO what happens at max aLength?
                    var part = upperText.AsSpan()[i..(i + aLength)];
                    if (part[0] == '.' || part[0] == ' ') { break; }
                    var nextOccurences = GetOccurences(upperText, part, 0, occurences);
                    if (nextOccurences.Count < 2 || part[^1] == '.' || part[^1] == ' ')
                    {
                        if (occurences != null)
                        {
                            if (possibleReplacements.Count > 0)
                            {
                                var possibleReplacement = (Replacement)null;
                                for (int replacementIndex = possibleReplacements.Count - 1; replacementIndex >= 0; replacementIndex--)
                                {
                                    if (AreOccurencesValid(inputSpan, processedInput, possibleReplacements[replacementIndex]))
                                    {
                                        possibleReplacement = possibleReplacements[replacementIndex];
                                        break;
                                    }
                                }

                                if (possibleReplacement != null)
                                {
                                    replacements.Add(possibleReplacement);
                                    possibleReplacement.Occurences.ForEach(x => allOccurences.Add(x));
                                    i += possibleReplacement.Length - 1;
                                }
                            }
                        }
                        break;
                    }
                    else
                    {
                        possibleReplacements.Add(new Replacement { Index = i, Length = aLength, Occurences = nextOccurences });
                        occurences = nextOccurences;
                    }
                }
            }

            return replacements
                .SelectMany(x => x.Occurences
                    .Select(o => (Occurence: processedInput.WordIndexes[o].Index, Replacement: new Replacement { Index = o, Length = x.Length })))
                .DistinctBy(x => x.Occurence) // TODO more efficient
                .ToDictionary(x => x.Occurence, kvp => kvp.Replacement);
        }

        private bool AreOccurencesValid(ReadOnlySpan<char> text, ProcessedInput processedInput, Replacement replacement)
        {
            var occurences = replacement.Occurences;
            var occurenceCount = occurences.Count;
            var phraseLength = replacement.Length;
            for (var i = 0; i < occurenceCount; i++)
            {
                for (var j = i + 1; j < occurenceCount; j++)
                {
                    for (var p = 0; p < phraseLength; p++)
                    {
                        var w1 = GetWord(text, processedInput, occurences, i, p);
                        var w2 = GetWord(text, processedInput, occurences, j, p);
                        if (char.IsUpper(w1[0]) || char.IsUpper(w2[0])) { continue; }
                        if (!MemoryExtensions.Equals(w1, w2, StringComparison.Ordinal)) { return false; }
                    }
                }
            }

            return true;
        }

        private static ReadOnlySpan<char> GetWord(ReadOnlySpan<char> text, ProcessedInput processedInput, List<int> occurences, int occurenceIndex, int wordIndex)
        {
            var w1Pointer = processedInput.WordIndexes[occurences[occurenceIndex] + wordIndex];
            var w1Range = w1Pointer.Index..(w1Pointer.Index + w1Pointer.Length);
            return text[w1Range];
        }

        private List<int> GetOccurences(ReadOnlySpan<char> text, ReadOnlySpan<char> part, int searchFrom, List<int> searchAtPositions)
        {
            // TODO validate occurences
            var occurences = new List<int>();
            if (searchAtPositions == null)
            {
                var partLength = part.Length;
                for (int pos = searchFrom; pos < text.Length - partLength; pos++)
                {
                    if (IsMatch(text, part, pos)) { occurences.Add(pos); }
                }
            }
            else
            {
                for (int i = 0; i < searchAtPositions.Count; i++)
                {
                    var pos = searchAtPositions[i];
                    if (pos < searchFrom || pos + part.Length > text.Length) { continue; }
                    if (IsMatch(text, part, pos, onlyCheckLastCharacter: true)) { occurences.Add(pos); }
                }
            }

            return occurences;
        }

        private bool IsMatch(ReadOnlySpan<char> text, ReadOnlySpan<char> part, int pos, bool onlyCheckLastCharacter = false)
        {
            var partLength = part.Length;
            var startIndex = onlyCheckLastCharacter ? partLength - 1 : 0;
            for (int i = startIndex; i < partLength; i++)
            {
                if (text[pos + i] != part[i])
                {
                    return false;
                }
            }
            return true;
        }

        private ProcessedInput GetSentences(string input)
        {
            var sentenceEnds = myDotRegex.Matches(input).OfType<Match>().Select(x => x.Index).ToArray();
            var noIndex = (-1, -1, true);
            var currentSentenceIndex = 0;
            var currentSentenceEnd = sentenceEnds[currentSentenceIndex];
            var sentenceBuilder = new StringBuilder();
            var wordIndexes = new List<(int Start, int Length, bool IsBorder)>();
            foreach (Match match in myAcronymPartRegex.Matches(input))
            {
                if (match.Index > currentSentenceEnd)
                {
                    currentSentenceEnd = sentenceEnds[currentSentenceIndex += 1];
                    sentenceBuilder.Append(". ");
                    wordIndexes.Add(noIndex);
                    wordIndexes.Add(noIndex);
                }

                var result = match.Groups["a"].Value;
                if (result.Length == 1)
                {
                    sentenceBuilder.Append(char.ToUpper(result[0]));
                    wordIndexes.Add((match.Index, match.Length, true));
                }
                else
                {
                    string acronymValue = match.Groups["b"].Value;
                    sentenceBuilder.Append(acronymValue);
                    wordIndexes.AddRange(Enumerable.Range(match.Index, acronymValue.Length).Select(x => (x, 1, x == match.Index + acronymValue.Length - 1)));
                }
            }
            sentenceBuilder.Append(".");
            wordIndexes.Add(noIndex);

            return new ProcessedInput { Acronym = sentenceBuilder.ToString(), WordIndexes = wordIndexes.ToArray(), PeriodIndexes = sentenceEnds };
        }

        private static readonly Regex myAcronymPartRegex = new Regex(@"(?:(?'a'[a-z])[a-z]+|(?'b'[A-Z]+))");
        private static readonly Regex myDotRegex = new Regex(@"\.");
    }
}
