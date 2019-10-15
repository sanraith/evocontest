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
    public sealed class MyEfficientSolution2
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
            var wordIndexes = processedInput.WordIndexes;
            var upperText = processedInput.Acronym;

            var replacements = new List<Replacement>();
            var allOccurences = new Dictionary<int, int>();
            for (int i = 0; i < upperText.Length; i++)
            {
                //TODO more efficient
                // Step over found occurrences
                if (allOccurences.TryGetValue(i, out var stepSize))
                {
                    i += stepSize - 1;
                    continue;
                }

                // Do not start mid acronym
                if (i > 0 && !wordIndexes[i - 1].IsNormalWordOrEndOfAcronym) { continue; }

                // Do not start on period
                if (wordIndexes[i].Length == -1) { continue; }

                List<int> occurences = null;
                var possibleReplacements = new List<Replacement>();
                for (var aLength = minLength; aLength <= maxLength && aLength <= upperText.Length - i; aLength++)
                {
                    if (!wordIndexes[i + aLength - 1].IsNormalWordOrEndOfAcronym) { continue; } // Do not end mid acronym
                    if (upperText[i + aLength - 1] == '.') { break; } // Do not go through a sentence

                    //var part = upperText.AsSpan()[i..(i + aLength)];
                    var part = new string(upperText.AsSpan()[i..(i + aLength)]);
                    occurences = GetOccurences(upperText, part, 0, occurences);
                    if (occurences.Count > 1)
                    {
                        possibleReplacements.Add(new Replacement { Index = i, Length = aLength, Occurences = occurences });
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
                        if (AreOccurencesValid(input, processedInput, possibleReplacements[replacementIndex]))
                        {
                            possibleReplacement = possibleReplacements[replacementIndex];
                            break;
                        }
                    }

                    if (possibleReplacement != null)
                    {
                        replacements.Add(possibleReplacement);
                        possibleReplacement.Occurences.ForEach(x => allOccurences.Add(x, possibleReplacement.Length));
                        i += possibleReplacement.Length - 1;
                    }
                }
            }

            return replacements
                .SelectMany(x => x.Occurences
                    .Select(o => (Occurence: processedInput.WordIndexes[o].Index, Replacement: new Replacement { Index = o, Length = x.Length })))
                .DistinctBy(x => x.Occurence) // TODO more efficient
                .ToDictionary(x => x.Occurence, kvp => kvp.Replacement);
        }

        private bool AreOccurencesValid(string text, ProcessedInput processedInput, Replacement replacement)
        {
            var wordIndexes = processedInput.WordIndexes;
            var occurences = replacement.Occurences;
            var occurenceCount = occurences.Count;
            var phraseLength = replacement.Length;

            // TODO parallelize?
            //for (var i = 0; i < occurenceCount; i++)
            //{
            //    for (var j = i + 1; j < occurenceCount; j++)
            //    {
            //        // TODO move to getOccurences and add sentenceEndCheck
            //        if (occurences[i] - 1 >= 0 && !wordIndexes[occurences[i] - 1].IsNormalWordOrEndOfAcronym) { return false; }
            //        if (occurences[j] - 1 >= 0 && !wordIndexes[occurences[j] - 1].IsNormalWordOrEndOfAcronym) { return false; }
            //        if (!wordIndexes[occurences[i] + phraseLength - 1].IsNormalWordOrEndOfAcronym) { return false; }
            //        if (!wordIndexes[occurences[j] + phraseLength - 1].IsNormalWordOrEndOfAcronym) { return false; }

            //        for (var p = 0; p < phraseLength; p++)
            //        {
            //            var w1 = GetWord(text, processedInput, occurences, i, p);
            //            var w2 = GetWord(text, processedInput, occurences, j, p);
            //            if (char.IsUpper(w1[0]) || char.IsUpper(w2[0])) { continue; }
            //            if (!MemoryExtensions.Equals(w1, w2, StringComparison.Ordinal)) { return false; }
            //        }
            //    }
            //}
            //return true;

            var result = Parallel.For(0, occurenceCount, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (i, loopState) =>
            {
                for (var j = i + 1; j < occurenceCount; j++)
                {
                    // TODO move to getOccurences and add sentenceEndCheck
                    if (occurences[i] - 1 >= 0 && !wordIndexes[occurences[i] - 1].IsNormalWordOrEndOfAcronym) { loopState.Stop(); return; }
                    if (occurences[j] - 1 >= 0 && !wordIndexes[occurences[j] - 1].IsNormalWordOrEndOfAcronym) { loopState.Stop(); return; }
                    if (!wordIndexes[occurences[i] + phraseLength - 1].IsNormalWordOrEndOfAcronym) { loopState.Stop(); return; }
                    if (!wordIndexes[occurences[j] + phraseLength - 1].IsNormalWordOrEndOfAcronym) { loopState.Stop(); return; }

                    for (var p = 0; p < phraseLength; p++)
                    {
                        if (loopState.IsStopped) { return; }
                        var w1 = GetWord(text, processedInput, occurences, i, p);
                        var w2 = GetWord(text, processedInput, occurences, j, p);
                        if (char.IsUpper(w1[0]) || char.IsUpper(w2[0])) { continue; }
                        if (!MemoryExtensions.Equals(w1, w2, StringComparison.Ordinal)) { loopState.Stop(); return; }
                    }
                }
            });
            return result.IsCompleted;
        }

        private static ReadOnlySpan<char> GetWord(ReadOnlySpan<char> text, ProcessedInput processedInput, List<int> occurences, int occurenceIndex, int wordIndex)
        {
            var w1Pointer = processedInput.WordIndexes[occurences[occurenceIndex] + wordIndex];
            var w1Range = w1Pointer.Index..(w1Pointer.Index + w1Pointer.Length);
            return text[w1Range];
        }

        private List<int> GetOccurences(string text, string part, int searchFrom, List<int> searchAtPositions)
        {
            var occurences = new List<int>();
            if (searchAtPositions == null)
            {
                var partLength = part.Length;
                var bag = new ConcurrentBag<List<int>>();
                //var rangePartitioner = Partitioner.Create(searchFrom, text.Length - partLength, (int)Math.Ceiling(text.Length / 4.0));
                //var parallelResult = Parallel.ForEach(rangePartitioner, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (range, loopState, loopIndex) =>
                // {
                //     var (from, to) = range;
                var (from, to) = (searchFrom, text.Length - partLength);
                var smallOccurenceParts = new List<int>();
                for (int pos = from; pos < to; pos++)
                {
                    if (IsMatch(text, part, pos)) { smallOccurenceParts.Add(pos); }
                }
                bag.Add(smallOccurenceParts);
                //});
                occurences = bag.SelectMany(x => x).ToList();
            }
            else
            {
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

                    case char _ when !isInWord && char.IsLower(c):
                        isInWord = true;
                        wordStart = index;
                        break;

                    case char _ when !isInAcronym && char.IsUpper(c):
                        isInAcronym = true;
                        wordStart = index;
                        break;

                    default: // In the middle of a word or acronym
                        break;
                }
            }

            return new ProcessedInput { Acronym = sentenceBuilder.ToString(), WordIndexes = wordIndexes.ToArray() };
        }

        private static readonly (int Min, int Max)[] myPredictedPhraseLengths = "2,3;3,6;3,8;4,10;5,12;7,15;10,20;12,26;14,30;14,34;17,40;20,50;14,55;14,70;20,100;30,150"
            .Split(";").Select(x => x.Split(',').Select(x => Convert.ToInt32(x)).ToList()).Select(x => (Min: x[0], Max: x[1])).ToArray();
    }
}
