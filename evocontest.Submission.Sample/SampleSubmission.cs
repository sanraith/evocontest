﻿using evocontest.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace evocontest.Submission.Sample
{
    public sealed class SampleSubmission : ISolution
    {
        public string Solve(string input)
        {
            var text = input;
            var acronyms = GetPossibleAcronyms(text);

            // Filter conflicting phrases
            acronyms.Where(x => AreConflicting(x.Value)).ToList().ForEach(x => acronyms.Remove(x.Key));

            // Remove expressions with only 1 occurrence
            acronyms.Where(kvp => kvp.Value.Count < 2).ToList().ForEach(x => acronyms.Remove(x.Key));

            // Replace expressions with acronyms
            foreach (var (acronym, phrases) in acronyms.OrderByDescending(x => x.Key.Length))
            {
                foreach (var phrase in phrases.Distinct())
                {
                    var replaceRegex = new Regex($"(?<![a-zA-Z]){phrase}(?![a-zA-Z])"); // no letter touching the phrase
                    text = replaceRegex.Replace(text, acronym);
                }
            }

            return text;
        }

        private static Dictionary<string, List<string>> GetPossibleAcronyms(string text)
        {
            var sentences = text.Split('.').Select(sentence => sentence.Trim()).Select(sentence => GetWords(sentence)).ToList();
            var acronyms = new Dictionary<string, List<string>>();
            foreach (var sentence in sentences)
            {
                for (var startIndex = 0; startIndex < sentence.Length; startIndex++)
                {
                    var word = sentence[startIndex];
                    if (IsAcronym(word))
                    {
                        AddAcronym(acronyms, word, word);
                    }

                    for (var endIndex = startIndex + 1; endIndex < sentence.Length; endIndex++)
                    {
                        string[] words = sentence[startIndex..(endIndex + 1)];
                        string acronym = string.Concat(words.Select(GetAcronymPart));
                        string phrase = string.Join(' ', words);
                        AddAcronym(acronyms, acronym, phrase);
                    }
                }
            }

            return acronyms;
        }

        private static bool AreConflicting(IEnumerable<string> phrases)
        {
            // [aaa BC, axx bb cc] => [[aaa, B, C], [axx, bb, cc]]
            var splitPhrases = phrases.Select(phrase => GetWords(phrase)
                    .SelectMany(word => IsAcronym(word) ? word.Select(c => c.ToString()) : new[] { word })
                    .ToList()
                ).ToList();

            for (var i = 0; i < splitPhrases.Count; i++)
            {
                for (var j = i + 1; j < splitPhrases.Count; j++)
                {
                    var (firstPhrase, secondPhrase) = (splitPhrases[i], splitPhrases[j]);
                    for (var k = 0; k < firstPhrase.Count; k++)
                    {
                        var (firstWord, secondWord) = (firstPhrase[k], secondPhrase[k]);
                        if (!IsAcronym(firstWord) && !IsAcronym(secondWord) && firstWord != secondWord)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static void AddAcronym(IDictionary<string, List<string>> possibleAcronyms, string acronym, string phrase)
        {
            if (!possibleAcronyms.ContainsKey(acronym))
            {
                possibleAcronyms.Add(acronym, new List<string>());
            }
            possibleAcronyms[acronym].Add(phrase);
        }

        private static string[] GetWords(string phrase) => phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        private static string GetAcronymPart(string word) => IsAcronym(word) ? word : char.ToUpper(word[0]).ToString();

        private static bool IsAcronym(string word) => word.All(char.IsUpper);
    }
}
