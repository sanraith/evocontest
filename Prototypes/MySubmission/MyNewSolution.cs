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
            IDictionary<Expression, int> occurences;
            do
            {
                var sentences = GetSentences(text);
                occurences = GetMultipleOccurringExpressions(sentences);
                text = ReplaceExpressionsWithAcronyms(text, occurences.Keys);
                PrintState(text, occurences);
            } while (occurences.Any());

            return text;
        }

        private static List<string> GetSentences(string text)
        {
            return text.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }

        private static IDictionary<Expression, int> GetMultipleOccurringExpressions(List<string> sentences)
        {
            // Gather expression occurrences.
            var occurrences = new ConcurrentDictionary<Expression, int>();
            foreach (var sentence in sentences)
            {
                var words = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                for (var index = 0; index < words.Length; index++)
                {
                    for (var length = 2; length <= words.Length - index; length++)
                    {
                        var expression = new Expression(words[index..(index + length)]);
                        occurrences.AddOrUpdate(expression, 1, (_, count) => count + 1);
                    }
                }
            }

            // Keep only expressions with multiple occurrences.
            occurrences
                .Where(kvp => kvp.Value <= 1).ToList()
                .ForEach(x => occurrences.Remove(x.Key, out _));

            return occurrences;
        }

        private static string ReplaceExpressionsWithAcronyms(string text, IEnumerable<Expression> expressions)
        {
            foreach (var expression in expressions)
            {
                var expressionString = expression.ToString();
                text = text.Replace(expressionString, expression.Acronym, StringComparison.OrdinalIgnoreCase);
            }

            return text;
        }

        private static void PrintState(string text, IDictionary<Expression, int> occurrences)
        {
            occurrences.ToList().ForEach(kvp => Console.WriteLine($"{kvp.Key}: {kvp.Value}"));
            if (occurrences.Any()) { Console.WriteLine(text); }
        }
    }
}
