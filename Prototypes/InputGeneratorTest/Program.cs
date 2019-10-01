using System;
using System.Linq;
using System.Diagnostics;
using evocontest.Runner.Common.Generator;
using MySubmission;
using evocontest.Submission.Sample;
using evocontest.Submission.Runner.Core;

namespace InputGeneratorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Test4();
        }

        private static void Test4()
        {
            var runner = new SubmissionRunner(typeof(SimulatedEfficientSolution))
            {
                ShouldValidateResult = false,
                Seed = 1
            };
            runner.Run();
        }

        private static void Test3()
        {
            var generatorConfig = new InputGeneratorConfig
            {
                Seed = 1337,
                InputLength = 10 * 1024 * 1024,
                //SentenceLength = new MinMaxPair(int.MaxValue, int.MaxValue),
                SentenceLength = new MinMaxPair(5, 10),
                PhraseCount = new MinMaxPair(2048),
                WordLength = new MinMaxPair(6, 10),
                PhraseLength = new MinMaxPair(3, 6)
            };

            var generator = new InputGenerator(generatorConfig);
            var pair = generator.Generate();
            //Console.WriteLine(pair.Input);
            Console.WriteLine(pair.Input.Length);
            Console.WriteLine();
            //Console.WriteLine(pair.Solution);
            Console.WriteLine(pair.Solution.Length);
            Console.WriteLine();

            var solver = new SampleSubmission();
            var result = solver.Solve(pair.Input);

            //Console.WriteLine(result);
            Console.WriteLine(result == pair.Solution);
        }
    }
}
