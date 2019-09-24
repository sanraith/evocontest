using evocontest.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySubmission
{
    class MyNewSolution2 : ISolution
    {
        public string Solve(string input)
        {
            var text = input;

            while (true)
            {
                var sentences = GetSentences(text);
                var possibleAcronyms = new ConcurrentDictionary<string, (List<string> Words, int Count)>();
                foreach (var sentence in sentences)
                {
                    var sentenceWords = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    for (var index = 0; index < sentenceWords.Length; index++)
                    {
                        for (var length = 2; length <= sentenceWords.Length - index; length++)
                        {
                            var acronymWords = sentenceWords[index..(index + length)].ToList();
                            var acronym = string.Concat(acronymWords.Select(GetAcronymPart));
                            possibleAcronyms.AddOrUpdate(acronym, (acronymWords, 1), (_, x) => (x.Words, x.Count + 1));
                        }
                    }
                }

                foreach (var word in sentences.SelectMany(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries)).Where(x => x.All(char.IsUpper)))
                {
                    if (possibleAcronyms.ContainsKey(word))
                    {
                        possibleAcronyms.AddOrUpdate(word, (new List<string>(), 1), (_, x) => (x.Words, x.Count + 1));
                    }
                }

                // Remove expressions with only 1 occurrence.
                possibleAcronyms.Where(kvp => kvp.Value.Count < 2).ToList().ForEach(x => possibleAcronyms.Remove(x.Key, out _));
                if (!possibleAcronyms.Any()) { break; }

                foreach (var acronym in possibleAcronyms.OrderByDescending(x => x.Value.Words.Count))
                {
                    text = text.Replace(string.Join(' ', acronym.Value.Words), acronym.Key);
                }
            }

            return text;
        }

        private static List<string> GetSentences(string text) => text.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

        private static string GetAcronymPart(string word) => word.All(x => char.IsUpper(x)) ? word : word[0..1].ToUpper();
    }
}
