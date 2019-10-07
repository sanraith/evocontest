using evocontest.Runner.Common.Extensions;
using evocontest.Runner.Common.Generator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace evocontest.Runner.Host.Common.Utility
{
    public sealed class TestDataManager
    {
        public string TestDataDirectory { get; set; } = "testdata";

        public TestDataManager(int seed, IInputGeneratorManager inputGeneratorManger)
        {
            mySeed = seed;
            myInputGeneratorManger = inputGeneratorManger;
        }

        public void Clean()
        {
            foreach (var directory in myDirectoriesToCleanup)
            {
                try
                {
                    directory.Delete(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not delete \"{directory.FullName}\". {ex.Message}");
                }
            }
            myDirectoriesToCleanup.Clear();
        }

        public GeneratorResult GetTestData(int difficulty, int targetIndex)
        {
            var targetDirectory = new DirectoryInfo(Path.Combine(TestDataDirectory, mySeed.ToString(), difficulty.ToString(), targetIndex.ToString()));
            var inputFileInfo = new FileInfo(Path.Combine(targetDirectory.FullName, "input.txt"));
            var solutionFileInfo = new FileInfo(Path.Combine(targetDirectory.FullName, "solution.txt"));
            var configFileInfo = new FileInfo(Path.Combine(targetDirectory.FullName, "config.txt"));

            if (!targetDirectory.Exists)
            {
                GenerateAllInputData(difficulty, targetIndex + 1);
            }

            var input = File.ReadAllText(inputFileInfo.FullName);
            var solution = File.ReadAllText(solutionFileInfo.FullName);
            var config = JsonSerializer.Deserialize<InputGeneratorConfig>(File.ReadAllText(configFileInfo.FullName));

            return new GeneratorResult { Input = input, Solution = solution, Config = config };
        }

        private void GenerateAllInputData(int difficulty, int count)
        {
            Console.Write($"Generating test data... ");
            var difficultyDirectory = new DirectoryInfo(Path.Combine(TestDataDirectory, mySeed.ToString(), difficulty.ToString()));
            var seedDirectory = new DirectoryInfo(Path.Combine(TestDataDirectory, mySeed.ToString()));
            if (!myDirectoriesToCleanup.Any(x => x.FullName == seedDirectory.FullName))
            {
                myDirectoriesToCleanup.Add(seedDirectory);
            }

            var sw = Stopwatch.StartNew();
            var results = myInputGeneratorManger.Generate(difficulty, count);
            foreach (var (result, i) in results.WithIndex())
            {
                var targetDirectory = new DirectoryInfo(Path.Combine(difficultyDirectory.FullName, i.ToString()));
                targetDirectory.Create();
                var newInputFileInfo = new FileInfo(Path.Combine(targetDirectory.FullName, "input.txt"));
                var newSolutionFoleInfo = new FileInfo(Path.Combine(targetDirectory.FullName, "solution.txt"));
                var newConfigFileInfo = new FileInfo(Path.Combine(targetDirectory.FullName, "config.txt"));

                File.WriteAllText(newInputFileInfo.FullName, result.Input);
                File.WriteAllText(newSolutionFoleInfo.FullName, result.Solution);
                File.WriteAllText(newConfigFileInfo.FullName, JsonSerializer.Serialize(result.Config));
                if (i == 0)
                {
                    var length = result.Input.Length;
                    var chunkText = length < 1024 ? $"{length} byte" : $"{ length / 1024} KByte";
                    Console.Write($"{count} * {chunkText} chunks... ");
                }
                Console.Write("#");
            }
            Console.WriteLine($" in {sw.ElapsedMilliseconds} ms.");
        }

        private readonly int mySeed;
        private readonly IInputGeneratorManager myInputGeneratorManger;
        private readonly List<DirectoryInfo> myDirectoriesToCleanup = new List<DirectoryInfo>();
    }
}
