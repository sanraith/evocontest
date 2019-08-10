using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PerformanceTest
{
    internal sealed class PerformanceTestProgram
    {
        static void Main(string[] args)
        {
            var input = "A PDSC(Patient Data Service Client) segítségével lehet csatlakozni a Patient Data Servicehez. A világ legjobb dolga a PDS(Patient Data Service). Tehát a PDS Client az egyik leghasznosabb dolog.";
            Console.WriteLine(input);
            var solved = Solve(input);

            var benchmark = new Benchmark.Core.Benchmark(Spin);
            var result = benchmark.Run();
            Console.WriteLine(result);
        }

        private static void Spin()
        {
            const double c = 987.654;
            double number = 12345.6789;
            for (int i = 0; i < 1000000; i++)
            {
                number *= c;
                number /= c;
            }
        }

        public static string Solve(string input)
        {
            var replacements = new Dictionary<string, string>();
            var matches = new Regex(@"(?'short'\w+)\((?'long'[^\)]*)\)").Matches(input);
            foreach (Match match in matches)
            {
                var to = match.Groups["short"].Value;
                var from = match.Groups["long"].Value;
                replacements[from] = to;
            }
            var text = new Regex(@"\([^\)]*\)").Replace(input, string.Empty);

            var hasReplacedAnything = true;
            while (hasReplacedAnything)
            {
                Console.WriteLine(text);

                hasReplacedAnything = false;
                foreach (var (from, to) in replacements.OrderByDescending(x => x.Key.Length).ToList())
                {
                    var newText = text.Replace(from, to);
                    hasReplacedAnything |= text.Length != newText.Length;
                    text = newText;
                    hasReplacedAnything |= ReplaceInKeys(replacements, from, to);

                    if (hasReplacedAnything) { break; }
                }
            }

            return text;
        }

        private static bool ReplaceInKeys(Dictionary<string, string> replacements, string from, string to)
        {
            var hasReplacedAnything = false;
            foreach (var kvp in replacements.Where(k => k.Key.Contains(from) && k.Value != to).ToList())
            {
                hasReplacedAnything = true;
                replacements.Remove(kvp.Key);
                replacements.Add(kvp.Key.Replace(from, to), kvp.Value);
            }

            return hasReplacedAnything;
        }
    }
}
