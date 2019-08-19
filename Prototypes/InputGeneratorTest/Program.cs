using System;
using System.Diagnostics;
using evorace.Runner.Common.Generator;
using MySubmission;

namespace InputGeneratorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var generatorConfig = new InputGeneratorConfig
            {
                Seed = 1337,
                InputLength = 180,
                WordLength = new MinMaxPair(2, 5),
                SentenceLength = new MinMaxPair(16, 120)
            };
            var generator = new StructuredInputGenerator(generatorConfig);
            var result = generator.Generate2(1);
            Console.WriteLine(result.Input);

            var solved = new MySolution().Solve(result.Input);
            Console.WriteLine(solved);
            //var solved = new MySolution().Solve("A(BB CC). BB(DDD EEE). DDD FFFF DDD FFFF DDD FFFF DDD FFFF DDD FFFF DDD FFFF DDD FFFF DDD FFFF DDD FFFF A CC CC CC CC CC CC CC CC CC. EEE(FFFF A).");

            RunPerformanceTest();
        }

        private static void RunPerformanceTest()
        {
            var totalSw = Stopwatch.StartNew();
            for (var i = 15; i < 25; i++)
            {
                var sw = Stopwatch.StartNew();
                var maxTreeLevel = (int)Math.Pow(2, i);
                var generatorConfig = new InputGeneratorConfig
                {
                    Seed = 1337,
                    InputLength = maxTreeLevel,
                    WordLength = new MinMaxPair(2, 10),
                    SentenceLength = new MinMaxPair(16, 120)
                };
                var generator = new StructuredInputGenerator(generatorConfig);
                var result = generator.Generate2(maxTreeLevel);
                sw.Stop();
                Console.WriteLine($"Length: {result.Input.Length * 2.0 / 1024 / 1024:0.00} Mb Time: {sw.ElapsedMilliseconds}");
            }

            Console.WriteLine($"Total: {totalSw.ElapsedMilliseconds}");
        }
    }
}
