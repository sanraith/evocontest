﻿using evocontest.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

    [DebuggerDisplay("{Index}..{Index + Length}: {Occurrences.Length} occurrences")]
    public sealed class Replacement
    {
        public int Index;
        public int Length;
        public List<int> Occurrences;
    }

    [DebuggerDisplay("{Acronym}, {WordIndexes.Length}")]
    public sealed class ProcessedInput
    {
        public string Acronym;
        public (int Index, int Length, bool IsNormalWordOrEndOfAcronym)[] WordIndexes;
    }

    public sealed class MyEfficientSolution : ISolution
    {
        public string Solve(string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            ThreadPool.SetMaxThreads(4, 4);
            var processedInput = GetProcessedInput(input);
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
            var predictedDifficultyLevel = Math.Max(0, (int)Math.Round(Math.Log2(input.Length / 256.0), 0, MidpointRounding.AwayFromZero));
            var minLength = predictedDifficultyLevel == 0 ? 2 : myPredictedPhraseLengths[predictedDifficultyLevel - 1].Min;
            var maxLength = predictedDifficultyLevel == 0 ? 12 : myPredictedPhraseLengths[predictedDifficultyLevel + 1].Max;

            var inputSpan = input.AsSpan();
            var wordIndexes = processedInput.WordIndexes;
            var upperText = processedInput.Acronym;
            var upperTextSpan = upperText.AsSpan();
            var upperTextMemory = upperText.AsMemory();

            var replacements = new List<Replacement>();
            var allOccurrences = new int[upperTextSpan.Length];
            for (int i = 0; i < upperTextSpan.Length; i++)
            {
                // Step over found occurrences
                var stepSize = allOccurrences[i];
                if (stepSize != 0)
                {
                    i += stepSize - 1;
                    continue;
                }

                // Do not start mid acronym
                if (i > 0 && !wordIndexes[i - 1].IsNormalWordOrEndOfAcronym) { continue; }

                // Do not start on period
                if (wordIndexes[i].Length == -1) { continue; }

                List<int> currentOccurrences = null;
                var possibleReplacements = new List<Replacement>();
                if (!upperTextSpan[i..Math.Min(upperTextSpan.Length, i + minLength)].Contains('.'))
                {
                    int skippedCharCount = 0;
                    var currentMaxLength = Math.Min(maxLength, upperTextSpan.Length - i);
                    for (var aLength = minLength; aLength <= currentMaxLength; aLength++)
                    {
                        if (upperTextSpan[i + aLength - 1] == '.') { break; } // Do not go through a sentence
                        if (!wordIndexes[i + aLength - 1].IsNormalWordOrEndOfAcronym) { skippedCharCount++; continue; } // Do not end mid acronym

                        var part = upperTextMemory[i..(i + aLength)];
                        currentOccurrences = GetOccurrences(upperText, part, skippedCharCount, currentOccurrences);
                        skippedCharCount = 0;
                        if (currentOccurrences.Count > 1)
                        {
                            possibleReplacements.Add(new Replacement { Index = i, Length = aLength, Occurrences = currentOccurrences });
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (possibleReplacements.Count > 0)
                {
                    var possibleReplacement = (Replacement)null;
                    for (int replacementIndex = possibleReplacements.Count - 1; replacementIndex >= 0; replacementIndex--)
                    {
                        if (AreOccurrencesValid(inputSpan, processedInput, possibleReplacements[replacementIndex]))
                        {
                            possibleReplacement = possibleReplacements[replacementIndex];
                            break;
                        }
                    }

                    if (possibleReplacement != null)
                    {
                        replacements.Add(possibleReplacement);

                        // possibleOccurrences.SelectMany(x => x).ToList().ForEach(allOccurrences.Add);
                        var replacementLength = possibleReplacement.Length;
                        var possibleOccurrences = possibleReplacement.Occurrences;
                        var possibleOccurrenceLength = possibleOccurrences.Count;
                        for (var oIndex = 0; oIndex < possibleOccurrenceLength; oIndex++)
                        {
                            allOccurrences[possibleOccurrences[oIndex]] = replacementLength;
                        }
                        i += possibleReplacement.Length - 1;
                    }
                }
            }

            return replacements
                .SelectMany(x => x.Occurrences
                    .Select(o => (Occurrence: processedInput.WordIndexes[o].Index, Replacement: new Replacement { Index = o, Length = x.Length })))
                .DistinctBy(x => x.Occurrence)
                .ToDictionary(x => x.Occurrence, kvp => kvp.Replacement);
        }

        private bool AreOccurrencesValid(ReadOnlySpan<char> text, ProcessedInput processedInput, Replacement replacement)
        {
            var wordIndexes = processedInput.WordIndexes;
            var occurrences = replacement.Occurrences;
            var occurrenceCount = occurrences.Count;
            var phraseLength = replacement.Length;

            for (var i = 0; i < occurrenceCount; i++)
            {
                if (occurrences[i] - 1 >= 0 && !wordIndexes[occurrences[i] - 1].IsNormalWordOrEndOfAcronym) { return false; }
                if (!wordIndexes[occurrences[i] + phraseLength - 1].IsNormalWordOrEndOfAcronym) { return false; }
                for (var j = i + 1; j < occurrenceCount; j++)
                {
                    if (occurrences[j] - 1 >= 0 && !wordIndexes[occurrences[j] - 1].IsNormalWordOrEndOfAcronym) { return false; }
                    if (!wordIndexes[occurrences[j] + phraseLength - 1].IsNormalWordOrEndOfAcronym) { return false; }

                    for (var p = 0; p < phraseLength; p++)
                    {
                        var w1 = GetWord(text, processedInput, occurrences, i, p);
                        var w2 = GetWord(text, processedInput, occurrences, j, p);
                        if (IsUpper(w1[0]) || IsUpper(w2[0])) { continue; }
                        if (!w1.Equals(w2, StringComparison.Ordinal)) { return false; }
                    }
                }
            }
            return true;
        }

        private static ReadOnlySpan<char> GetWord(ReadOnlySpan<char> text, ProcessedInput processedInput, List<int> occurrences, int occurrenceIndex, int wordIndex)
        {
            var w1Pointer = processedInput.WordIndexes[occurrences[occurrenceIndex] + wordIndex];
            return text.Slice(w1Pointer.Index, w1Pointer.Length);
        }

        private List<int> GetOccurrences(string text, ReadOnlyMemory<char> part, int skippedCharCount, List<int> searchAtPositions)
        {
            List<int> occurrences;
            if (searchAtPositions == null)
            {
                var partLength = part.Length;
                var bags = new List<int>[4];
                var rangePartitioner = Partitioner.Create(0, text.Length - partLength, (int)Math.Ceiling(text.Length / 4.0));
                var parallelResult = Parallel.ForEach(rangePartitioner, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (range, loopState, loopIndex) =>
                {
                    var partSpan = part.Span;
                    var (from, to) = range;
                    to = Math.Min(text.Length, to + part.Length - 1);
                    var smallOccurrenceParts = new List<int>();
                    var textSpan = text.AsSpan();

                    var pos = -1;
                    var currentSpan = textSpan[from..to];
                    while ((pos = currentSpan.IndexOf(partSpan, StringComparison.Ordinal)) != -1)
                    {
                        from += pos;
                        smallOccurrenceParts.Add(from);
                        from += 1;
                        currentSpan = textSpan[from..to];
                    }
                    bags[loopIndex] = smallOccurrenceParts;
                });
                occurrences = bags.Where(x => x != null).SelectMany(x => x).ToList();
            }
            else
            {
                var partSpan = part.Span;
                occurrences = new List<int>();
                for (int i = 0; i < searchAtPositions.Count; i++)
                {
                    var pos = searchAtPositions[i];
                    if (pos + part.Length > text.Length) { continue; }
                    if (IsMatch(text, partSpan, pos, skippedCharCount)) { occurrences.Add(pos); }
                }
            }

            return occurrences;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsMatch(ReadOnlySpan<char> text, ReadOnlySpan<char> part, int pos, int skippedCharCount)
        {
            var partLength = part.Length;
            var startIndex = partLength - 1 - skippedCharCount;
            if (skippedCharCount == 0)
            {
                return text[pos + startIndex] == part[startIndex];
            }
            else
            {
                return text[(pos + startIndex)..(pos + partLength)].Equals(part[startIndex..partLength], StringComparison.Ordinal);
            }
        }

        private ProcessedInput GetProcessedInput(string input)
        {
            var inputLength = input.Length;
            var isInWord = false;
            var isInAcronym = false;
            var noIndex = (-1, -1, true);
            var upperBuilder = new StringBuilder();
            var wordIndexes = new List<(int Start, int Length, bool IsBorder)>();
            var wordStart = -1;

            void HandleWordEnd(int wordEnd)
            {
                var wordLength = wordEnd - wordStart + 1;
                if (isInWord)
                {
                    upperBuilder.Append(char.ToUpper(input[wordStart]));
                    wordIndexes.Add((wordStart, wordLength, true));
                }
                else
                {
                    upperBuilder.Append(input[wordStart..(wordEnd + 1)]);
                    for (int x = wordStart; x < wordStart + wordLength; x++)
                    {
                        wordIndexes.Add((x, 1, x == wordEnd));
                    }
                }
                isInWord = false;
                isInAcronym = false;
            }

            for (var index = 0; index < inputLength; index++)
            {
                var c = input[index];
                switch (c)
                {
                    case '.':
                        HandleWordEnd(index - 1);
                        upperBuilder.Append(".");
                        wordIndexes.Add(noIndex);
                        break;

                    case ' ' when isInWord || isInAcronym:
                        HandleWordEnd(index - 1);
                        break;

                    case char _ when !isInWord && IsLower(c):
                        isInWord = true;
                        wordStart = index;
                        break;

                    case char _ when !isInAcronym && IsUpper(c):
                        isInAcronym = true;
                        wordStart = index;
                        break;

                    default: // In the middle of a word or acronym
                        break;
                }
            }

            return new ProcessedInput { Acronym = upperBuilder.ToString(), WordIndexes = wordIndexes.ToArray() };
        }

        private static bool IsUpper(char c)
        {
            return c > 64 && c < 91;
        }

        private static bool IsLower(char c)
        {
            return c > 96 && c < 123;
        }

        private static readonly (int Min, int Max)[] myPredictedPhraseLengths = "2,3;3,6;3,8;4,10;5,12;7,15;10,20;12,26;14,30;14,34;17,40;20,50;14,55;14,70;20,100;30,150"
            .Split(";").Select(x => x.Split(',').Select(x => Convert.ToInt32(x)).ToList()).Select(x => (Min: x[0], Max: x[1])).ToArray();
    }
}
