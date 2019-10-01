using System;
using System.Linq;
using System.Diagnostics;
using evocontest.Runner.Common.Generator;
using MySubmission;
using evocontest.Submission.Sample;
using evocontest.Submission.Runner.Core;
using evocontest.Runner.Common.Extensions;

namespace InputGeneratorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test4();
            Test5();
        }

        private static void Test5()
        {
            var myRandom = new Random(0);
            var difficultyLevel = 0;
            var length = (int)Math.Pow(2, difficultyLevel) * 128;
            var length2 = (int)Math.Pow(2, 5) * 128;
            var length3 = (int)Math.Pow(2, 10) * 128;
            var length4 = (int)Math.Pow(2, 15) * 128;

            var configs = new[]
            {
                new InputGeneratorConfig                // 0
                {
                    //Seed = myRandom.Next(),
                    InputLength = length,

                    WordLength = new MinMaxPair(6, 10),
                    SentenceLength = new MinMaxPair(5, Math.Max(10, length / 50)),

                    PhraseLength = new MinMaxPair(2, Math.Max(10, length / 500)),
                    PhraseCount = new MinMaxPair(Math.Max(1, length / 2000), Math.Max(1, length / 100)),
                    PhraseCollapseChance = .1,
                    PhraseRepeatCount = new MinMaxPair(2, Math.Max(2, length / 1000)),

                    DecoyPhraseCount = new MinMaxPair(Math.Max(1, length / 2000), Math.Max(1, length / 200)),
                    DecoyRepeatCount = new MinMaxPair(2, Math.Max(2, length / 10000))
                },

                new InputGeneratorConfig                // 5
                {
                    //Seed = myRandom.Next(),
                    InputLength = length2,

                    WordLength = new MinMaxPair(6, 10),
                    SentenceLength = new MinMaxPair(5, Math.Max(10, length2 / 50)),

                    PhraseLength = new MinMaxPair(2, Math.Max(10, length2 / 500)),
                    PhraseCount = new MinMaxPair(Math.Max(1, length2 / 400), Math.Max(1, length2 / 200)),
                    PhraseCollapseChance = .1,
                    PhraseRepeatCount = new MinMaxPair(2, Math.Max(2, length2 / 1000)),

                    DecoyPhraseCount = new MinMaxPair(Math.Max(1, length2 / 1000), Math.Max(1, length2 / 500)),
                    DecoyRepeatCount = new MinMaxPair(2, Math.Max(2, length2 / 10000))


                    //InputLength = length2,

                    //WordLength = new MinMaxPair(6, 10),
                    //SentenceLength = new MinMaxPair(Math.Max(10, length2 / 100), Math.Max(10, length2 / 50)),

                    //PhraseLength = new MinMaxPair(2, Math.Max(5, (int)Math.Sqrt(length2 / 200))),
                    //PhraseCount = new MinMaxPair(Math.Max(1, length2 / 900), Math.Max(1, length2/600)),
                    //PhraseCollapseChance = .1,
                    //PhraseRepeatCount = new MinMaxPair(2, Math.Max(2,(int)Math.Sqrt( length2 / 5000))),

                    //DecoyPhraseCount =new MinMaxPair(Math.Max(1, length2 / 1600), Math.Max(1, length2/1300)),// new MinMaxPair(0),// new MinMaxPair(Math.Max(1, length2 / 1000), Math.Max(1, length2 / 500)),
                    //DecoyRepeatCount = new MinMaxPair(2, Math.Max(2,(int)Math.Sqrt( length2 / 8000))),
                },

                new InputGeneratorConfig                // 10
                {
                    //Seed = myRandom.Next(),
                    InputLength = length3,

                    WordLength = new MinMaxPair(6, 10),
                    SentenceLength = new MinMaxPair(Math.Max(10, length3 / 100), Math.Max(10, length3 / 50)),

                    PhraseLength = new MinMaxPair(2, Math.Max(5, (int)Math.Sqrt(length3 / 200))),
                    PhraseCount = new MinMaxPair(Math.Max(1, length3 / 900), Math.Max(1, length3/600)),
                    PhraseCollapseChance = .1,
                    PhraseRepeatCount = new MinMaxPair(2, Math.Max(2,(int)Math.Sqrt( length3 / 5000))),

                    DecoyPhraseCount =new MinMaxPair(Math.Max(1, length3 / 1600), Math.Max(1, length3/1300)),// new MinMaxPair(0),// new MinMaxPair(Math.Max(1, length3 / 1000), Math.Max(1, length3 / 500)),
                    DecoyRepeatCount = new MinMaxPair(2, Math.Max(2,(int)Math.Sqrt( length3 / 8000))),//new MinMaxPair(0), //new MinMaxPair(2, Math.Max(2, length3 / 10000))
                },

                new InputGeneratorConfig                // 15
                {
                    //Seed = myRandom.Next(),
                    InputLength = length4,

                    WordLength = new MinMaxPair(6, 10),
                    SentenceLength = new MinMaxPair(Math.Max(10, length4 / 100), Math.Max(10, length4 / 50)),

                    PhraseLength = new MinMaxPair(2, Math.Max(5, (int)Math.Sqrt(length4 / 200))),
                    PhraseCount = new MinMaxPair(Math.Max(1, length4 / 7500), Math.Max(1, length4/6000)),
                    PhraseCollapseChance = .1,
                    PhraseRepeatCount = new MinMaxPair(2, Math.Max(2,(int)Math.Sqrt( length4 / 8000))),

                    DecoyPhraseCount =new MinMaxPair(Math.Max(1, length4 / 8000), Math.Max(1, length4/7500)),// new MinMaxPair(0),// new MinMaxPair(Math.Max(1, length4 / 1000), Math.Max(1, length4 / 500)),
                    DecoyRepeatCount = new MinMaxPair(2, Math.Max(2,(int)Math.Sqrt( length4 / 10000))),//new MinMaxPair(0), //new MinMaxPair(2, Math.Max(2, length4 / 10000))
                }
            };

            // TODO too few acronym for low difficulty
            // TODO size increase not granual enough
            foreach (var (config, index) in configs.WithIndex())
            {
                Console.WriteLine($"TargetLength: {config.InputLength}");
                for (int i = 0; i < 10; i++)
                {
                    config.Seed = myRandom.Next();
                    var generator = new InputGenerator(config);
                    var result = generator.Generate();
                    Console.WriteLine($"{index}, {result.Input.Length}");
                }
            }

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
