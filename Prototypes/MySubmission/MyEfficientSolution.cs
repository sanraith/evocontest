using evocontest.Common;
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

    [DebuggerDisplay("{Index}..{Index + Length}: {Occurences.Length} occurences")]
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
            var predictedDifficultyLevel = Math.Max(0, (int)Math.Round(Math.Log2(input.Length / 256.0), 0, MidpointRounding.AwayFromZero));
            var minLength = predictedDifficultyLevel == 0 ? 2 : myPredictedPhraseLengths[predictedDifficultyLevel - 1].Min;
            var maxLength = predictedDifficultyLevel == 0 ? 12 : myPredictedPhraseLengths[predictedDifficultyLevel + 1].Max;

            var inputSpan = input.AsSpan();
            var inputMemory = input.AsMemory();
            var wordIndexes = processedInput.WordIndexes;
            var upperText = processedInput.Acronym;

            var replacements = new List<Replacement>();
            var allOccurrences = new int[upperText.Length];
            for (int i = 0; i < upperText.Length; i++)
            {
                //TODO more efficient
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

                // TODO period not filtered!!!
                List<int> currentOccurrences = null;
                var possibleReplacements = new List<Replacement>();
                for (var aLength = minLength; aLength <= maxLength && aLength <= upperText.Length - i; aLength++)
                {
                    if (upperText[i + aLength - 1] == '.') { break; } // Do not go through a sentence
                    if (!wordIndexes[i + aLength - 1].IsNormalWordOrEndOfAcronym) { continue; } // Do not end mid acronym

                    //var part = upperText.AsSpan()[i..(i + aLength)];
                    var part = new string(upperText.AsSpan()[i..(i + aLength)]);
                    currentOccurrences = GetOccurences(upperText, part, 0, currentOccurrences);
                    if (currentOccurrences.Count > 1)
                    {
                        possibleReplacements.Add(new Replacement { Index = i, Length = aLength, Occurrences = currentOccurrences });
                    }
                    else
                    {
                        break;
                    }
                }
                if (possibleReplacements.Count > 0)
                {
                    var possibleReplacement = (Replacement)null;
                    for (int replacementIndex = possibleReplacements.Count - 1; replacementIndex >= 0; replacementIndex--)
                    {
                        if (AreOccurencesValid(inputMemory, inputSpan, processedInput, possibleReplacements[replacementIndex]))
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
                    .Select(o => (Occurence: processedInput.WordIndexes[o].Index, Replacement: new Replacement { Index = o, Length = x.Length })))
                .DistinctBy(x => x.Occurence) // TODO more efficient
                .ToDictionary(x => x.Occurence, kvp => kvp.Replacement);
        }

        private bool AreOccurencesValid(ReadOnlyMemory<char> textMemory, ReadOnlySpan<char> text, ProcessedInput processedInput, Replacement replacement)
        {
            var wordIndexes = processedInput.WordIndexes;
            var occurrences = replacement.Occurrences;
            var occurenceCount = occurrences.Count;
            var phraseLength = replacement.Length;

            // TODO parallelize?
            for (var i = 0; i < occurenceCount; i++)
            {
                var occurrenceI = occurrences[i];
                if (occurrenceI - 1 >= 0 && !wordIndexes[occurrenceI - 1].IsNormalWordOrEndOfAcronym) { return false; }
                if (!wordIndexes[occurrenceI + phraseLength - 1].IsNormalWordOrEndOfAcronym) { return false; }

                var iWords = new ReadOnlyMemory<char>[phraseLength];
                for (var p = 0; p < phraseLength; p++)
                {
                    var w1Pointer = wordIndexes[occurrenceI + p];
                    if (w1Pointer.Length == -1)
                    {
                        return false;
                    }
                    iWords[p] = textMemory.Slice(w1Pointer.Index, w1Pointer.Length);
                }

                for (var j = i + 1; j < occurenceCount; j++)
                {
                    var occurrenceJ = occurrences[j];

                    // TODO move to getOccurences and add sentenceEndCheck
                    if (occurrenceJ - 1 >= 0 && !wordIndexes[occurrenceJ - 1].IsNormalWordOrEndOfAcronym) { return false; }
                    if (!wordIndexes[occurrenceJ + phraseLength - 1].IsNormalWordOrEndOfAcronym) { return false; }

                    for (var p = 0; p < phraseLength; p++)
                    {
                        var w1 = iWords[p].Span;
                        var w2 = GetWord(text, processedInput, occurrenceJ, p);
                        if (IsUpper(w1[0]) || IsUpper(w2[0])) { continue; }
                        if (!MemoryExtensions.Equals(w1, w2, StringComparison.Ordinal)) { return false; }
                    }
                }
            }

            return true;
        }

        private static ReadOnlySpan<char> GetWord(ReadOnlySpan<char> text, ProcessedInput processedInput, int occurrenceStartIndex, int wordIndex)
        {
            var w1Pointer = processedInput.WordIndexes[occurrenceStartIndex + wordIndex];
            return text.Slice(w1Pointer.Index, w1Pointer.Length);
        }

        private List<int> GetOccurences(string text, string part, int searchFrom, List<int> searchAtPositions)
        {
            List<int> occurences;
            if (searchAtPositions == null)
            {
                var partLength = part.Length;
                var bags = new List<int>[4];
                var rangePartitioner = Partitioner.Create(searchFrom, text.Length - partLength, (int)Math.Ceiling(text.Length / 4.0));
                var parallelResult = Parallel.ForEach(rangePartitioner, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (range, loopState, loopIndex) =>
                {
                    var (from, to) = range;
                    var smallOccurenceParts = new List<int>();
                    for (int pos = from; pos < to; pos++)
                    {
                        if (IsMatch(text, part, pos)) { smallOccurenceParts.Add(pos); }
                    }
                    bags[loopIndex] = smallOccurenceParts;
                });
                occurences = new List<int>(bags[0].Count + bags[1].Count + (bags[2]?.Count ?? 0) + (bags[3]?.Count ?? 0));
                for (int bagIndex = 0; bagIndex < 4; bagIndex++)
                {
                    var bag = bags[bagIndex];
                    if (bag == null) { break; }
                    var bagLength = bag.Count;
                    for (int itemIndex = 0; itemIndex < bagLength; itemIndex++)
                    {
                        occurences.Add(bag[itemIndex]);
                    }
                }
            }
            else
            {
                occurences = new List<int>();
                // TODO parallelize?
                for (int i = 0; i < searchAtPositions.Count; i++)
                {
                    var pos = searchAtPositions[i];
                    if (pos < searchFrom || pos + part.Length > text.Length) { continue; }
                    if (IsMatch(text, part, pos, onlyCheckLastCharacter: true)) { occurences.Add(pos); }
                }
            }

            return occurences;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsMatch(ReadOnlySpan<char> text, ReadOnlySpan<char> part, int pos, bool onlyCheckLastCharacter = false)
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
            var inputLength = input.Length;
            var isInWord = false;
            var isInAcronym = false;
            var noIndex = (-1, -1, true);
            var sentenceBuilder = new StringBuilder();
            var wordIndexes = new List<(int Start, int Length, bool IsBorder)>();
            var wordStart = -1;

            void HandleWordEnd(int wordEnd)
            {
                var wordLength = wordEnd - wordStart + 1;
                if (isInWord)
                {
                    sentenceBuilder.Append(char.ToUpper(input[wordStart]));
                    wordIndexes.Add((wordStart, wordLength, true));
                }
                else
                {
                    sentenceBuilder.Append(input[wordStart..(wordEnd + 1)]);
                    wordIndexes.AddRange(Enumerable.Range(wordStart, wordLength).Select(x => (x, 1, x == wordEnd)));
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
                        sentenceBuilder.Append(".");
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

            return new ProcessedInput { Acronym = sentenceBuilder.ToString(), WordIndexes = wordIndexes.ToArray() };
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
