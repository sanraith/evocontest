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
            Test2();
        }

        private static void Test2()
        {
            var generatorConfig = new InputGeneratorConfig
            {
                Seed = 1337,
                InputLength = 1024 * 1024 * 5,
                WordLength = new MinMaxPair(2, 5),
                SentenceLength = new MinMaxPair(16, 120)
            };

            GeneratorResult result;
            int treeLevel = 0;
            do
            {
                treeLevel++;
                var generator = new StructuredInputGenerator(generatorConfig);
                result = generator.Generate(treeLevel);
                Console.WriteLine(treeLevel);
            } while (result.Input.Length < generatorConfig.InputLength / 2);
            //Console.WriteLine(result.Input);
            var solved = new MySolution().Solve(result.Input);
            //Console.WriteLine(solved);
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
                var result = generator.Generate(maxTreeLevel);
                sw.Stop();
                Console.WriteLine($"Length: {result.Input.Length * 2.0 / 1024 / 1024:0.00} Mb Time: {sw.ElapsedMilliseconds}");
            }

            Console.WriteLine($"Total: {totalSw.ElapsedMilliseconds}");
        }
    }
}
