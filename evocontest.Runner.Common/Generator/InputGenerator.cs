using evocontest.Runner.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace evocontest.Runner.Common.Generator
{
    public sealed class InputGenerator : InputGeneratorBase
    {
        public InputGenerator(InputGeneratorConfig config) : base(config)
        {
            Init();
        }

        public GeneratorResult Generate()
        {
            Init();

            myPhrases = GenerateNormalPhrases();

            myDecoyPhrases = GenerateDecoyPhrases();

            var skeleton = GenerateSkeleton();
            var sentenceLengths = GetSlices(skeleton.Count, myConfig.SentenceLength);

            var solution = GenerateSolution(skeleton, sentenceLengths);

            var input = GenerateInput(skeleton, sentenceLengths);

            //Console.WriteLine($"AcronymPhrases: {myPhrases.Count}, DecoyPhrases: {myDecoyPhrases.Count}, Junk: {myExtraJunkCount}, AllPhrases: {skeleton.Count}, Words: {myWordSet.Count}");

            return new GeneratorResult
            {
                Input = input,
                Solution = solution,
                Config = myConfig
            };
        }

        private string GenerateInput(IEnumerable<Phrase> phrases, IEnumerable<int> sentenceLengths)
        {
            var renderedPhrases = new HashSet<Phrase>();
            var endPoints = new HashSet<int>();
            _ = sentenceLengths.Aggregate(0, (sum, length) => { sum += length; endPoints.Add(sum); return sum; });

            var sb = new StringBuilder();
            var isFirst = true;
            foreach (var (phrase, phraseIndex) in phrases.WithIndex())
            {
                if (isFirst) { isFirst = false; } else { sb.Append(' '); }
                switch (phrase)
                {
                    case DecoyPhrase dp:
                    case JunkPhrase jp:
                        sb.AppendJoin(' ', phrase.Words);
                        break;
                    case NormalPhrase np:
                        var shouldCollapse = myRandom.NextDouble() < myConfig.PhraseCollapseChance;
                        if (renderedPhrases.Add(np) || !shouldCollapse)
                        {
                            sb.AppendJoin(' ', np.Words);
                        }
                        else
                        {
                            var wordArray = np.Words.ToArray();
                            var slices = GetSlices(wordArray.Length, new MinMaxPair(2, wordArray.Length));
                            var parts = new List<string>();
                            var pos = 0;
                            foreach (var slice in slices)
                            {
                                var words = wordArray[pos..(pos + slice)];
                                parts.Add(Phrase.GetSingleWordOrAcronym(words));
                                pos += slice;
                            }
                            sb.AppendJoin(' ', parts);
                        }
                        break;
                }
                if (endPoints.Contains(phraseIndex + 1))
                {
                    sb.Append('.');
                }
            }

            return sb.ToString();
        }

        private string GenerateSolution(List<Phrase> phrases, IEnumerable<int> sentenceLengths)
        {
            var endPoints = new HashSet<int>();
            _ = sentenceLengths.Aggregate(0, (sum, length) => { sum += length; endPoints.Add(sum); return sum; });

            var sb = new StringBuilder();
            var isFirst = true;
            foreach (var (phrase, phraseIndex) in phrases.WithIndex())
            {
                if (isFirst) { isFirst = false; } else { sb.Append(' '); }
                if (phrase is NormalPhrase)
                {
                    sb.Append(phrase.Acronym);
                }
                else
                {
                    sb.AppendJoin(' ', phrase.Words);
                }
                if (endPoints.Contains(phraseIndex + 1))
                {
                    sb.Append('.');
                }
            }

            return sb.ToString();
        }

        private List<Phrase> GenerateSkeleton()
        {
            var phrasesToUse = new List<Phrase>();
            foreach (var phrase in myPhrases)
            {
                var repeatCount = GetRandomFromRange(myConfig.PhraseRepeatCount);
                for (int i = 0; i < repeatCount; i++)
                {
                    phrasesToUse.Add(phrase);
                    myCurrentLength += phrase.Length;
                    if (i > 0 && myCurrentLength > myConfig.InputLength * .9) { break; }
                }
            }
            phrasesToUse.AddRange(myDecoyPhrases);
            phrasesToUse.ShuffleInPlace(myRandom);

            var length = phrasesToUse.Sum(x => x.Words.Count + x.Words.Sum(x => x.Length));
            var lengthToFill = Math.Max(0, myConfig.InputLength - length);
            var junkPhraseCount = phrasesToUse.Count - 1;
            var avgJunkWordLength = (myConfig.WordLength.Min + myConfig.WordLength.Max) / 2.0;
            var junkWordCount = Math.Max(junkPhraseCount, (int)Math.Ceiling(lengthToFill / (avgJunkWordLength + 1)));
            myExtraJunkCount = junkWordCount;

            var minimumSlices = Enumerable.Repeat(1, junkPhraseCount);
            var additionSlices = GetSlices(junkWordCount - junkPhraseCount, new MinMaxPair(0, myConfig.PhraseLength.Max));
            var sliceDiff = junkPhraseCount - additionSlices.Count;
            var slices = additionSlices
                .Concat(Enumerable.Repeat(0, sliceDiff > 0 ? sliceDiff : 0))
                .Zip(minimumSlices, (a, b) => a + b)
                .ToList()
                .ShuffleInPlace(myRandom);

            var result = new List<Phrase>();
            for (int wordIndex = 0; wordIndex < phrasesToUse.Count - 1; wordIndex++)
            {
                var sliceLength = slices[wordIndex];
                Phrase junkPhrase;
                bool isAllowed = false;
                do
                {
                    var words = GeneratePhraseWords(sliceLength);
                    junkPhrase = new JunkPhrase(words);
                    if (myValidAcronymSet.Contains(junkPhrase.Acronym))
                    {
                        continue;
                    }

                    var testedAcronym = string.Concat(phrasesToUse[wordIndex].Acronym[1..],
                        junkPhrase.Acronym, phrasesToUse[wordIndex + 1].Acronym[..^1]);
                    isAllowed = myValidAcronymSet.All(x => !testedAcronym.Contains(x));
                } while (!isAllowed);

                result.Add(phrasesToUse[wordIndex]);
                result.Add(junkPhrase);
            }
            result.Add(phrasesToUse[^1]);

            return result;
        }

        private List<Phrase> GenerateDecoyPhrases()
        {
            var decoyCount = GetRandomFromRange(myConfig.DecoyPhraseCount);
            var decoyPhrases = new List<Phrase>();

            for (int decoyIndex = 0; decoyIndex < decoyCount; decoyIndex++)
            {
                if (myCurrentLength > myConfig.InputLength * .7) { break; }

                Phrase phrase;
                do
                {
                    phrase = new DecoyPhrase(GeneratePhraseWords());
                } while (myValidAcronymSet.Any(x => phrase.Acronym.Contains(x) || x.Contains(phrase.Acronym)));
                myConflictingAcronymSet.Add(phrase.Acronym);
                decoyPhrases.Add(phrase);
                myCurrentLength += phrase.Length;

                var decoyRepeatCount = GetRandomFromRange(myConfig.DecoyRepeatCount);
                for (int repeatIndex = 0; repeatIndex < decoyRepeatCount; repeatIndex++)
                {
                    var similarPhrase = new DecoyPhrase(phrase.Words.Select(GenerateSimilarNewWord));
                    decoyPhrases.Add(similarPhrase);
                    myCurrentLength += similarPhrase.Length;
                }
            }

            return decoyPhrases;
        }

        private List<Phrase> GenerateNormalPhrases()
        {
            var phrases = new List<Phrase>();
            var phraseCount = GetRandomFromRange(myConfig.PhraseCount);
            for (int phraseIndex = 0; phraseIndex < phraseCount; phraseIndex++)
            {
                if (myCurrentLength > myConfig.InputLength) { break; }
                Phrase phrase;
                do
                {
                    phrase = new NormalPhrase(GeneratePhraseWords());
                } while (myValidAcronymSet.Any(x => phrase.Acronym.Contains(x) || x.Contains(phrase.Acronym)) || !myValidAcronymSet.Add(phrase.Acronym));
                phrases.Add(phrase);
                myCurrentLength += phrase.Length;
            }

            return phrases;
        }

        private List<string> GeneratePhraseWords(int? wordCount = null)
        {
            var phraseWords = new List<string>();
            wordCount ??= GetRandomFromRange(myConfig.PhraseLength);
            for (int wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                var wordLength = GetRandomFromRange(myConfig.WordLength);
                phraseWords.Add(GenerateNewWord(wordLength));
            }

            return phraseWords;
        }

        private string GenerateNewWord(int wordLength)
        {
            string word;
            do
            {
                word = GenerateWord(wordLength);
            } while (!myWordSet.Add(word));
            return word;
        }

        private string GenerateSimilarNewWord(string word)
        {
            Span<char> similarWordSpan = word.ToArray();
            string similarWord;
            do
            {
                var pos = myRandom.Next(1, word.Length);
                similarWordSpan[pos] = (char)(((similarWordSpan[pos] - 96) % 25) + 97);
                similarWord = similarWordSpan.ToString();
            } while (myWordSet.Contains(similarWord));

            return similarWord;
        }

        private IList<int> GetSlices(int totalLength, MinMaxPair sliceLength)
        {
            var slices = new List<int>();
            while (totalLength > 0)
            {
                var slice = GetRandomFromRange(sliceLength, totalLength);
                totalLength -= slice;
                slices.Add(slice);
            }

            return slices.ShuffleInPlace(myRandom);
        }

        private void Init()
        {
            myCurrentLength = 0;
            myPhrases = new List<Phrase>();

            myWordSet = new HashSet<string>();
            myValidAcronymSet = new HashSet<string>();

            myDecoyPhrases = new List<Phrase>();
            myConflictingAcronymSet = new HashSet<string>();
        }

        private int myExtraJunkCount;
        private int myCurrentLength = 0;
        private List<Phrase> myPhrases;
        private HashSet<string> myWordSet;
        private HashSet<string> myValidAcronymSet;
        private List<Phrase> myDecoyPhrases;
        private HashSet<string> myConflictingAcronymSet;
    }
}
