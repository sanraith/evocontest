using evocontest.Runner.Common.Extensions;
using evocontest.Runner.Common.Generator;
using System.IO;

namespace evocontest.Submission.Runner.Core
{
    public class TestDataManager
    {
        public string TestDataDirectory { get; set; } = "testdata";

        public TestDataManager(int seed, InputGeneratorManager inputGeneratorManger)
        {
            mySeed = seed;
            myInputGeneratorManger = inputGeneratorManger;
        }

        public GeneratorResult GetTestData(int difficulty, int targetIndex)
        {
            var targetDirectory = new DirectoryInfo(Path.Combine(TestDataDirectory, mySeed.ToString(), difficulty.ToString(), targetIndex.ToString()));
            var inputFileInfo = new FileInfo(Path.Combine(targetDirectory.FullName, "input.txt"));
            var solutionFileInfo = new FileInfo(Path.Combine(targetDirectory.FullName, "solution.txt"));

            if (!targetDirectory.Exists)
            {
                GenerateAllInputData(difficulty, targetIndex + 1);
            }

            var input = File.ReadAllText(inputFileInfo.FullName);
            var solution = File.ReadAllText(solutionFileInfo.FullName);

            return new GeneratorResult { Input = input, Solution = solution };
        }

        private void GenerateAllInputData(int difficulty, int count)
        {
            System.Console.WriteLine($"Generating testdata...");
            var difficultyDirectory = new DirectoryInfo(Path.Combine(TestDataDirectory, mySeed.ToString(), difficulty.ToString()));

            var results = myInputGeneratorManger.Generate(difficulty, count);
            foreach (var (result, i) in results.WithIndex())
            {
                var targetDirectory = new DirectoryInfo(Path.Combine(difficultyDirectory.FullName, i.ToString()));
                targetDirectory.Create();
                var newInputFileInfo = new FileInfo(Path.Combine(targetDirectory.FullName, "input.txt"));
                var newSolutionFoleInfo = new FileInfo(Path.Combine(targetDirectory.FullName, "solution.txt"));

                File.WriteAllText(newInputFileInfo.FullName, result.Input);
                File.WriteAllText(newSolutionFoleInfo.FullName, result.Solution);
            }
        }

        private readonly int mySeed;
        private readonly InputGeneratorManager myInputGeneratorManger;
    }
}
