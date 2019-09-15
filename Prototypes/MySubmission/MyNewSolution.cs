using evorace.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MySubmission
{
    public class MyNewSolution : ISolution
    {
        // TODO No overlapping should be allowed

        public string Solve(string input)
        {
            var text = input;
            ConcurrentDictionary<Expression, int> occurences;

            do
            {
                var sentences = GetSentences(text);
                occurences = GetMultipleOccuringExpressions(sentences);
                text = ReplaceExpressionsWithAcronyms(text, occurences.Keys);
                PrintState(text, occurences);
            } while (occurences.Any());

            return text;
        }

        private static List<string> GetSentences(string text)
        {
            return text
                .Split('.', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();
        }

        private static ConcurrentDictionary<Expression, int> GetMultipleOccuringExpressions(List<string> sentences)
        {
            var occurences = new ConcurrentDictionary<Expression, int>();

            // Gather expression occurences.
            foreach (var sentence in sentences)
            {
                var words = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                for (var index = 0; index < words.Length; index++)
                {
                    for (var length = 2; length <= words.Length - index; length++)
                    {
                        var expression = new Expression(words[index..(index + length)]);
                        occurences.AddOrUpdate(expression, 1, (_, current) => current + 1);
                    }
                }
            }

            // Keep only expressions with multiple occurences.
            occurences
                .Where(kvp => kvp.Value <= 1).ToList()
                .ForEach(x => occurences.Remove(x.Key, out _));

            return occurences;
        }

        private static string ReplaceExpressionsWithAcronyms(string text, IEnumerable<Expression> expressions)
        {
            foreach (var expression in expressions)
            {
                var expressionString = expression.ToString();
                var acronym = string.Concat(expression.Words.Select(x => x[0..1])).ToUpperInvariant();
                text = text.Replace(expressionString, acronym, StringComparison.OrdinalIgnoreCase);
            }

            return text;
        }

        private static void PrintState(string text, ConcurrentDictionary<Expression, int> occurences)
        {
            occurences.ToList().ForEach(kvp => Console.WriteLine($"{kvp.Key}: {kvp.Value}"));
            if (occurences.Any()) { Console.WriteLine(text); }
        }
    }
}
