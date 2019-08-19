using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using evorace.Common;

namespace MySubmission
{
    public class MySolution : ISolution
    {
        public string Solve(string input)
        {
            var replacements = new Dictionary<string, string>();
            var matches = new Regex(@"(?'short'\w+)\((?'long'[^\)]*)\)").Matches(input);
            foreach (Match match in matches)
            {
                var to = match.Groups["short"].Value;
                var from = match.Groups["long"].Value;
                replacements[from] = to;
            }
            var duplicates = replacements.Where(kvp => replacements.Values.Count(v => v == kvp.Value) > 1).ToDictionary(x => x.Key, x => x.Value);
            var text = new Regex(@"\((?'from'[^\)]*)\)").Replace(input, x =>
                duplicates.ContainsKey(x.Groups["from"].Value) ? x.Value : string.Empty
            );
            duplicates.ToList().ForEach(x => replacements.Remove(x.Key));

            var hasReplacedAnything = true;
            while (hasReplacedAnything)
            {
                //Console.WriteLine(text);

                hasReplacedAnything = false;
                foreach (var (from, to) in replacements.OrderByDescending(x => x.Key.Length).ToList())
                {
                    hasReplacedAnything |= ReplaceInText(ref text, from, to);
                    hasReplacedAnything |= ReplaceInKeys(replacements, from, to);

                    if (hasReplacedAnything) { break; }
                }
            }

            return text;
        }

        private static bool ReplaceInText(ref string text, string from, string to)
        {
            var newText = text.Replace(from, to, StringComparison.OrdinalIgnoreCase);
            var hasReplacedAnything = text != newText;
            text = newText;

            return hasReplacedAnything;
        }

        private static bool ReplaceInKeys(Dictionary<string, string> replacements, string from, string to)
        {
            var hasReplacedAnything = false;
            foreach (var kvp in replacements.Where(k => k.Key.Contains(from, StringComparison.OrdinalIgnoreCase) && k.Value != to).ToList())
            {
                hasReplacedAnything = true;
                replacements.Remove(kvp.Key);
                replacements.Add(kvp.Key.Replace(from, to, StringComparison.OrdinalIgnoreCase), kvp.Value);
            }

            return hasReplacedAnything;
        }
    }
}
