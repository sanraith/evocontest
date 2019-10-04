using evocontest.Submission.Runner.Core;
using evocontest.Submission.Sample;
using System;

namespace evocontest.Submission.Runner
{
    public static class Program
    {
        /// <summary>
        /// Change this if you want to test a different implementation.
        /// </summary>
        public static Type SubmissionType { get; } = typeof(MySubmission);

        /// <summary>
        /// Change this if you want to use a different set of random test data.
        /// </summary>
        public static int Seed { get; } = 42;

        static void Main(string[] args)
        {
            Console.WriteLine($"--- {SubmissionType.FullName} ---");
            var submissionRunner = new SubmissionRunner(SubmissionType) { Seed = Seed };
            submissionRunner.Run();
        }
    }
}
