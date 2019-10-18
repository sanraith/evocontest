using evocontest.Common;
using evocontest.Runner.Common.Generator;
using System;
using System.Diagnostics;

namespace evocontest.Submission.Runner.Core
{
    public sealed class SubmissionRunner
    {
        /// <summary>
        /// Randomness seed for input generation.
        /// </summary>
        public int Seed { get; set; }

        /// <summary>
        /// Set it to false, if the validness of the submission result should not be checked.
        /// </summary>
        public bool ShouldValidateResult { get; set; } = true;

        public SubmissionRunner(Type submissionType)
        {
            mySubmissionType = submissionType;
        }

        public void Run()
        {
            const int roundLength = 20;
            const int difficultyCount = 16;
            const int maxRuntimeMillis = 5000;

            var manager = new TestDataManager(Seed, new InputGeneratorManager(Seed));

            try
            {
                var difficulty = -1;
                Stopwatch sw = new Stopwatch();
                long bestTime;
                int bestDifficulty;
                do
                {
                    bestTime = sw.ElapsedMilliseconds;
                    bestDifficulty = difficulty++;

                    // Init round
                    manager.GetTestData(difficulty, roundLength - 1);

                    // Warmup 
                    ((ISolution)Activator.CreateInstance(mySubmissionType)).Solve(string.Empty);

                    sw.Reset();
                    for (int index = 0; index < roundLength; index++)
                    {
                        var testData = manager.GetTestData(difficulty, index);

                        sw.Start();
                        var submission = (ISolution)Activator.CreateInstance(mySubmissionType);
                        var result = submission.Solve(testData.Input);
                        sw.Stop();

                        if (ShouldValidateResult && result != testData.Solution)
                        {
                            throw new Exception($"Invalid solution for input data! Difficulty: {difficulty}, Index: {index}, Seed: {Seed}");
                        }
                    }
                    Console.WriteLine($"Difficulty: {difficulty}, Time: {sw.ElapsedMilliseconds} ms");
                } while (difficulty < difficultyCount && sw.ElapsedMilliseconds < maxRuntimeMillis);

                Console.WriteLine();
                Console.WriteLine($"Best time is {bestTime} ms at difficulty {bestDifficulty}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private readonly Type mySubmissionType;
    }
}
