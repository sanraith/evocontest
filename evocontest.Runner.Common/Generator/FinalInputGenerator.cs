using System;
using System.Collections.Generic;
using System.Linq;

namespace evocontest.Runner.Common.Generator
{
    public sealed class FinalInputGenerator : InputGeneratorBase
    {
        public FinalInputGenerator(InputGeneratorConfig config) : base(config)
        {
            Init();
        }

        public void Generate()
        {
            Init();
            // Generate phrases
            // - Distinct acronyms
            // - No overlap
            // - No subset
            myPhrases = GeneratePhrases();

            // - Add partially conflicted acronyms

            // Generate decoy phrases
            // - May PARTIALLY overlap with normal phrases
            // - 1. not distinct acronym // should have minimal difference in them
            // - 2. normal acronym is part of it
            // - 3. same word as an acronym

            // Build text map
            // - Add >=2 occurence to all normal phrases
            // - Add >=2 occurence to decoy phrases
            // - Use multiple level of extraction (take extra care for decoy phrases)
            // - Add junk words between

            // Split them into sentences (respect phrase borders)
        }

        private List<Phrase> GenerateDecoys()
        {
            var decoyCount = 10; // GetRandomFromRange(myConfig.PhraseCount);
            var decoyPhrases = new List<Phrase>();
            for (int decoyIndex = 0; decoyIndex < decoyCount; decoyIndex++)
            {
                Phrase phrase;
                do
                {
                    phrase = GeneratePhrase();
                } while (!myValidAcronymSet.Add(phrase.Acronym));
            }

            return null;
        }

        private List<Phrase> GeneratePhrases()
        {
            var phrases = new List<Phrase>();
            var phraseCount = GetRandomFromRange(myConfig.PhraseCount);
            for (int phraseIndex = 0; phraseIndex < phraseCount; phraseIndex++)
            {
                Phrase phrase;
                do
                {
                    phrase = GeneratePhrase();
                } while (!myValidAcronymSet.Add(phrase.Acronym));
                phrases.Add(phrase);
            }

            return phrases;
        }

        private Phrase GeneratePhrase()
        {
            var phraseWords = new List<string>();
            var wordCount = GetRandomFromRange(myConfig.PhraseLength);
            for (int wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                var wordLength = GetRandomFromRange(myConfig.WordLength);
                string word;
                do
                {
                    word = GenerateWord(wordLength);
                } while (!myWordSet.Add(word));
                phraseWords.Add(word);
            }

            return new Phrase(phraseWords);
        }

        private void Init()
        {
            myPhrases = new List<Phrase>();
            myWordSet = new HashSet<string>();
            myValidAcronymSet = new HashSet<string>();
        }

        private List<Phrase> myPhrases;
        private HashSet<string> myWordSet;
        private HashSet<string> myValidAcronymSet;

        // ---------------------------------------
        private sealed class Phrase
        {
            public IReadOnlyList<string> Words { get; }

            public string Acronym { get; }

            public Phrase(IEnumerable<string> words)
            {
                Words = words.ToList();
                Acronym = string.Concat(Words.Select(x => x[0])).ToUpper();
                myHashCode = Words.Aggregate(0, (sum, word) => sum ^ word.GetHashCode());
            }

            public override bool Equals(object? obj)
            {
                if (obj is Phrase other)
                {
                    return Words.SequenceEqual(other.Words);
                }
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return myHashCode;
            }

            private readonly int myHashCode;
        }
    }
}
