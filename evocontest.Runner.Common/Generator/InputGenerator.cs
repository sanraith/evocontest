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

            // Generate phrases
            myPhrases = GenerateNormalPhrases();
            // - Add partially conflicted acronyms

            // Generate decoy phrases
            myDecoyPhrases = GenerateDecoyPhrases();
            // - 2. normal acronym is part of it
            // - 3. same word as an acronym

            // Build skeleton
            var skeleton = GenerateSkeleton();
            var sentenceLengths = GetSlices(skeleton.Count, myConfig.SentenceLength);

            // GenerateSolution
            var solution = GenerateSolution(skeleton, sentenceLengths);

            // Render
            var input = GenerateInput(skeleton, sentenceLengths);

            Console.WriteLine($"AcronymPhrases: {myPhrases.Count}, DecoyPhrases: {myDecoyPhrases.Count}, SinglePhrases: {skeleton.OfType<SinglePhrase>().Count()}, Junk: {myExtraJunkCount}, AllPhrases: {skeleton.Count}, Words: {myWordSet.Count}");

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
                    case SinglePhrase sp:
                    case DecoyPhrase dp:
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
                                parts.Add(Phrase.GetWordOrAcronym(words));
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

            phrasesToUse = phrasesToUse.ShuffleInPlace(myRandom).SelectMany(phrase =>
            {
                var wordLength = GetRandomFromRange(myConfig.WordLength);
                var junkWord = GenerateNewWord(wordLength);
                return new[] { phrase, new SinglePhrase(junkWord) };
            }).ToList();

            var length = phrasesToUse.Sum(x => x.Words.Count + x.Words.Sum(x => x.Length));
            var lengthToFill = Math.Max(0, myConfig.InputLength - length);
            var phraseCount = phrasesToUse.Count;
            var extraJunk = new Queue<(int Index, Phrase Phrase)>();
            while (lengthToFill > 0)
            {
                var wordLength = GetRandomFromRange(myConfig.WordLength);
                var junkWord = GenerateNewWord(wordLength);
                var pos = myRandom.Next(0, phraseCount);
                extraJunk.Enqueue((pos, new SinglePhrase(junkWord)));
                lengthToFill -= wordLength + 1;
            }
            extraJunk = new Queue<(int Index, Phrase Phrase)>(extraJunk.OrderBy(x => x.Index));
            myExtraJunkCount = extraJunk.Count;

            var result = new List<Phrase>();
            foreach (var (phrase, index) in phrasesToUse.WithIndex())
            {
                while (extraJunk.Count > 0 && extraJunk.Peek().Index == index)
                {
                    result.Add(extraJunk.Dequeue().Phrase);
                }
                result.Add(phrase);
            }

            return result;
        }

        private int myExtraJunkCount;

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
                } while (myValidAcronymSet.Contains(phrase.Acronym));
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
                } while (!myValidAcronymSet.Add(phrase.Acronym));
                phrases.Add(phrase);
                myCurrentLength += phrase.Length;
            }

            return phrases;
        }

        private List<string> GeneratePhraseWords()
        {
            var phraseWords = new List<string>();
            var wordCount = GetRandomFromRange(myConfig.PhraseLength);
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


        private int myCurrentLength = 0;
        private List<Phrase> myPhrases;
        private HashSet<string> myWordSet;
        private HashSet<string> myValidAcronymSet;
        private List<Phrase> myDecoyPhrases;
        private HashSet<string> myConflictingAcronymSet;
    }
}
