using System;
using System.Diagnostics;
using evorace.Runner.Common.Generator;

namespace InputGeneratorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //var generatorConfig = new InputGeneratorConfig
            //{
            //    Seed = 1337,
            //    InputLength = 180,
            //    WordLength = new MinMaxPair(2, 10),
            //    SentenceLength = new MinMaxPair(16, 120)
            //};
            //var generator = new InputGenerator(generatorConfig);
            //var result = generator.Generate();
            //Console.WriteLine(result.Input);

            var generatorConfig = new InputGeneratorConfig
            {
                Seed = 1337,
                InputLength = 180,
                WordLength = new MinMaxPair(2, 10),
                SentenceLength = new MinMaxPair(16, 120)
            };
            var generator = new StructuredInputGenerator(generatorConfig);
            var result = generator.Generate();
            Console.WriteLine(result.Input);

            //RunPerformanceTest();
        }

        private static void RunPerformanceTest()
        {
            var totalSw = Stopwatch.StartNew();
            for (var i = 19; i < 29; i++)
            {
                var sw = Stopwatch.StartNew();
                var length = (int)Math.Pow(2, i);
                var generatorConfig = new InputGeneratorConfig
                {
                    Seed = 1337,
                    InputLength = length,
                    WordLength = new MinMaxPair(2, 10),
                    SentenceLength = new MinMaxPair(16, 120)
                };
                var generator = new InputGenerator(generatorConfig);
                var result = generator.Generate();
                sw.Stop();
                Console.WriteLine($"Length: {length * 2.0 / 1024 / 1024} Mb Time: {sw.ElapsedMilliseconds}");
            }

            Console.WriteLine($"Total: {totalSw.ElapsedMilliseconds}");
        }
    }
}
