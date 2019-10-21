using evocontest.Runner.Common.Generator;
using evocontest.Submission.Test.Core;
using MySubmission;
using NUnit.Framework;

namespace evocontest.Submission.Test
{
    /// <summary>
    /// Tests your submission. 
    /// Test cases are defined in the base class.
    /// </summary>
    [TestFixture]
    public sealed class MyEfficientSolutionTest : SubmissionTestBase
    {
        public MyEfficientSolutionTest()
        {
            TestedType = typeof(MyEfficientSolution);
        }

        [Test]
        public void Solve_TrickyCase()
        {
            var seed = 1242597738;
            var difficultyLevel = 1;
            var generatorConfig = new DifficultyLevels().GetConfig(seed, difficultyLevel);
            var testData = new InputGenerator(generatorConfig).Generate();

            var result = Submission?.Solve(testData.Input);
            Assert.AreEqual(testData.Solution, result);
        }

        [Test]
        public void Solve_TrickyCase2()
        {
            var seed = 1448362612;
            var difficultyLevel = 1;
            var generatorConfig = new DifficultyLevels().GetConfig(seed, difficultyLevel);
            var testData = new InputGenerator(generatorConfig).Generate();

            var result = Submission?.Solve(testData.Input);
            Assert.AreEqual(testData.Solution, result);
        }

    }
}
