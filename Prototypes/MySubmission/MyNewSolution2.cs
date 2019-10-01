using evocontest.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySubmission
{
    class MyNewSolution2// : ISolution
    {
        public string Solve(string input)
        {
            var text = input;

            var sentences = GetSentences(text);
            var possibleAcronyms = new Dictionary<string, List<string>>();
            foreach (var sentence in sentences.Select(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries)))
            {
                for (var index = 0; index < sentence.Length; index++)
                {
                    for (var length = 2; length <= sentence.Length - index; length++)
                    {
                        var acronymWords = sentence[index..(index + length)].ToList();
                        var expression = string.Join(' ', acronymWords);
                        var acronym = string.Concat(acronymWords.Select(GetAcronymPart));
                        if (!possibleAcronyms.ContainsKey(acronym))
                        {
                            possibleAcronyms.Add(acronym, new List<string>());
                        }
                        possibleAcronyms[acronym].Add(expression);
                    }
                }
            }

            // Count existing acronyms
            sentences.SelectMany(GetWords)
                .Where(word => IsAcronym(word) && possibleAcronyms.ContainsKey(word)).ToList()
                .ForEach(word => possibleAcronyms[word].Add(word));

            // Remove expressions with only 1 occurrence
            possibleAcronyms.Where(kvp => kvp.Value.Count < 2).ToList().ForEach(x => possibleAcronyms.Remove(x.Key, out _));

            // Replace expressions with acronyms
            foreach (var acronym in possibleAcronyms.OrderByDescending(x => x.Key.Length))
            {
                foreach (var expression in acronym.Value.Distinct().OrderByDescending(x => x.Length))
                {
                    text = text.Replace(string.Join(' ', expression), acronym.Key);
                }
            }

            return text;
        }


        private static List<string> GetSentences(string text) => text.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

        private static string[] GetWords(string sentence) => sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        private static string GetAcronymPart(string word) => IsAcronym(word) ? word : word[0..1].ToUpper();

        private static bool IsAcronym(string word) => word.All(char.IsUpper);
    }
}
