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

            public (int Index, int Length)[] WordIndexes { get; set; }

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
            var replacements = GetReplacements(processedInput);
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

        private Dictionary<int, Replacement> GetReplacements(ProcessedInput processedInput)
        {
            List<Replacement> replacements = new List<Replacement>();
            var acronym = processedInput.Acronym;
            var minLength = 2;
            var maxLength = 26; // TODO
            HashSet<int> allOccurences = new HashSet<int>();
            for (int i = 0; i < acronym.Length; i++)
            {
                //TODO more efficient
                if (allOccurences.Contains(i))
                {
                    continue;
                }

                List<int> occurences = null;
                Replacement possibleReplacement = null;
                for (var aLength = minLength; aLength <= maxLength && aLength <= acronym.Length - i; aLength++)
                {
                    // TODO what happens at max aLength?
                    var part = acronym.AsSpan()[i..(i + aLength)];
                    if (part[0] == '.' || part[0] == ' ') { break; }
                    var (isValidOccurence, nextOccurences) = GetOccurences(acronym, part, i, occurences);
                    if (nextOccurences.Count < 2 || part[^1] == '.' || part[^1] == ' ')
                    {
                        if (occurences != null)
                        {
                            if (possibleReplacement != null)
                            {
                                replacements.Add(possibleReplacement);
                                possibleReplacement.Occurences.ForEach(x => allOccurences.Add(x));
                            }
                            i += aLength - 2;
                        }
                        break;
                    }
                    else
                    {
                        if (isValidOccurence)
                        {
                            possibleReplacement = new Replacement { Index = i, Length = aLength, Occurences = nextOccurences };
                        }
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

        private bool AreOccurencesValid(string text, ReadOnlySpan<char> part, List<(int, ProcessedInput)> occurences)
        {
            return true;
        }

        private (bool, List<int>) GetOccurences(ReadOnlySpan<char> text, ReadOnlySpan<char> part, int searchFrom, List<int> searchAtPositions)
        {
            // TODO validate occurences
            var occurences = new List<int>();
            if (searchAtPositions == null)
            {
                var partLength = part.Length;
                for (int pos = searchFrom; pos <= text.Length - partLength; pos++)
                {
                    if (IsMatch(text, part, pos)) { occurences.Add(pos); }
                }
            }
            else
            {
                for (int i = 0; i < searchAtPositions.Count; i++)
                {
                    var pos = searchAtPositions[i];
                    if (pos < searchFrom) { continue; }
                    if (IsMatch(text, part, pos, onlyCheckLastCharacter: true)) { occurences.Add(pos); }
                }
            }

            return (true, occurences);
        }

        private bool IsMatch(ReadOnlySpan<char> text, ReadOnlySpan<char> part, int pos, bool onlyCheckLastCharacter = false)
        {
            var partLength = part.Length;
            if (pos + partLength > text.Length) { return false; }
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
            var noIndex = (-1, -1);
            var currentSentenceIndex = 0;
            var currentSentenceEnd = sentenceEnds[currentSentenceIndex];
            var sentenceBuilder = new StringBuilder();
            var wordIndexes = new List<(int Start, int Length)>();
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
                    wordIndexes.Add((match.Index, match.Length));
                }
                else
                {
                    string acronymValue = match.Groups["b"].Value;
                    sentenceBuilder.Append(acronymValue);
                    wordIndexes.AddRange(Enumerable.Range(match.Index, acronymValue.Length).Select(x => (x, 1)));
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
