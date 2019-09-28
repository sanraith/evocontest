using evocontest.Runner.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace evocontest.Runner.Common.Generator
{
    public sealed class FinalInputGenerator : InputGeneratorBase
    {
        public FinalInputGenerator(InputGeneratorConfig config) : base(config)
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

            // GenerateSolution
            // TODO add dots...
            var solution = GenerateSolution(skeleton);

            // Render
            // - Use multiple level of extraction (take extra care for decoy phrases)
            // - Split them into sentences (respect phrase borders)
            var input = GenerateInput(skeleton);

            return new GeneratorResult
            {
                Input = input,
                Solution = solution
            };
        }

        private string GenerateInput(IEnumerable<Phrase> phrases)
        {
            var renderedPhrases = new HashSet<Phrase>();

            var sb = new StringBuilder();
            var isFirst = true;
            foreach (var phrase in phrases)
            {
                if (isFirst) { isFirst = false; } else { sb.Append(' '); }
                switch (phrase)
                {
                    case SinglePhrase sp:
                    case DecoyPhrase dp:
                        sb.AppendJoin(' ', phrase.Words);
                        break;
                    case NormalPhrase np:
                        if (renderedPhrases.Add(np))
                        {
                            // TODO keep
                        }
                        else
                        {
                            // TODO change 
                        }
                        sb.AppendJoin(' ', np.Words);
                        break;
                }
            }

            return sb.ToString();
        }

        private string GenerateSolution(IEnumerable<Phrase> phrases)
        {
            var sb = new StringBuilder();
            var isFirst = true;
            foreach (var phrase in phrases)
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
                }
            }
            phrasesToUse.AddRange(myDecoyPhrases);

            phrasesToUse = phrasesToUse.Shuffle(myRandom).SelectMany(phrase =>
            {
                var wordLength = GetRandomFromRange(myConfig.WordLength);
                var junkWord = GenerateNewWord(wordLength);
                return new[] { phrase, new SinglePhrase(junkWord) };
            }).ToList();

            // Generate junk words
            var length = phrasesToUse.Sum(x => x.Words.Count + x.Words.Sum(x => x.Length));
            var lengthToFill = Math.Max(0, myConfig.InputLength - length);
            while (lengthToFill > 0)
            {
                var wordLength = GetRandomFromRange(myConfig.WordLength);
                var junkWord = GenerateNewWord(wordLength);
                phrasesToUse.Add(new SinglePhrase(junkWord));
                lengthToFill -= wordLength + 1;
            }

            return phrasesToUse.ToList();
        }


        private List<Phrase> GenerateDecoyPhrases()
        {
            var decoyCount = 10;
            var decoyPhrases = new List<Phrase>();

            for (int decoyIndex = 0; decoyIndex < decoyCount; decoyIndex++)
            {
                Phrase phrase;
                do
                {
                    phrase = new DecoyPhrase(GeneratePhraseWords());
                } while (myValidAcronymSet.Contains(phrase.Acronym));
                myConflictingAcronymSet.Add(phrase.Acronym);
                decoyPhrases.Add(phrase);

                var similarPhrase = new DecoyPhrase(phrase.Words.Select(GenerateSimilarNewWord));
                decoyPhrases.Add(similarPhrase);
            }

            return decoyPhrases;
        }

        private List<Phrase> GenerateNormalPhrases()
        {
            var phrases = new List<Phrase>();
            var phraseCount = GetRandomFromRange(myConfig.PhraseCount);
            for (int phraseIndex = 0; phraseIndex < phraseCount; phraseIndex++)
            {
                Phrase phrase;
                do
                {
                    phrase = new NormalPhrase(GeneratePhraseWords());
                } while (!myValidAcronymSet.Add(phrase.Acronym));
                phrases.Add(phrase);
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

        private void Init()
        {
            myPhrases = new List<Phrase>();

            myWordSet = new HashSet<string>();
            myValidAcronymSet = new HashSet<string>();

            myDecoyPhrases = new List<Phrase>();
            myConflictingAcronymSet = new HashSet<string>();
        }

        public List<Phrase> myPhrases;
        public HashSet<string> myWordSet;
        public HashSet<string> myValidAcronymSet;
        public List<Phrase> myDecoyPhrases;
        public HashSet<string> myConflictingAcronymSet;
    }
}
