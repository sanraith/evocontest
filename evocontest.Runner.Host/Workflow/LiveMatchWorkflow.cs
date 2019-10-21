using evocontest.Runner.Common.Extensions;
using evocontest.Runner.Host.Common.Utility;
using evocontest.Runner.Host.Configuration;
using evocontest.Runner.Host.Core;
using evocontest.Runner.RaspberryPiUtilities;
using evocontest.WebApp.Common;
using evocontest.WebApp.Common.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using DownloadedSubmission = evocontest.Runner.Host.Core.MatchManager.DownloadedSubmission;

namespace evocontest.Runner.Host.Workflow
{
    public sealed class LiveMatchWorkflow : IResolvable
    {
        public LiveMatchWorkflow(FileManager fileManager, IFanControl fanControl, MatchManager matchManager, HostConfiguration config)
        {
            myFileManager = fileManager;
            myFanControl = fanControl;
            myMatchManager = matchManager;
            myConfig = config;
        }

        public async Task RunAsync(string matchFilePath)
        {
            myFanControl.FanPower(true);
            //await ConsoleUtilities.CountDown(myConfig.CoolDownSeconds, i => $"Processzor lehűtése... {i}", "Processzor lehűtve.");
            Console.Clear();

            myFileManager.CleanTempDirectory();
            if (!TryGetFileInfo(matchFilePath, out var matchZipFile)) { return; }

            var workingDirectory = ExtractZipContents(matchZipFile);
            var matchMetadata = await GetMatchMetadata(workingDirectory);

            Console.WriteLine("Résztvevők: ");
            Console.WriteLine();
            foreach (var submission in matchMetadata.Submissions.OrderBy(x => x.UserName))
            {
                Console.WriteLine($"  {submission.UserName.PadRight(matchMetadata.Submissions.Max(x => x.UserName.Length))} - {submission.UploadDate.ToString("yyyy. MM. dd. HH:mm:ss")}");
            }
            Console.WriteLine();
            Console.Write("Mehet? (y | N) ");
            var confirm = Console.ReadLine();
            Console.WriteLine();
            if (!confirm.StartsWith("y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Megszakítva.");
                return;
            }

            var downloadedSubmissions = await SortSubmissionAssemblies(matchMetadata, workingDirectory);

            var result = await RunMatch(downloadedSubmissions);
            SaveMatchResult(result);
        }

        private async Task<MatchContainer> RunMatch(List<DownloadedSubmission> downloadedSubmissions)
        {
            MatchContainer lastResult = null!;
            var submissionsById = downloadedSubmissions.Select(x => x.Data).ToDictionary(x => x.Id, x => x);

            var results = new List<DownloadedSubmission>();

            myMatchManager.OnSubmissionEnd += MyMatchManager_OnSubmissionEnd;

            await foreach (var currentResult in myMatchManager.RunMatch(downloadedSubmissions))
            {
                lastResult = currentResult;
                var items = currentResult.Measurements
                    .Where(x => !myCompletedSubmissions.Contains(submissionsById[x.SubmissionId]) || x.Rounds.Count == currentResult.Measurements.Max(x => x.Rounds.Count))
                    .Select(x => ("   " + submissionsById[x.SubmissionId].UserName, x.Rounds.Last().TotalMilliseconds))
                    .OrderBy(x => x.Item2).ToList();

                Console.Clear();
                Console.WriteLine();
                //Console.WriteLine();
                DrawTimeDiagram(currentResult.Measurements.Max(x => x.Rounds.Count) - 1, 25, items);
                Console.WriteLine();
                DrawResultsTable(currentResult, downloadedSubmissions.Count - downloadedSubmissions.Count(x => x.Data.IsAdmin));
                Console.ReadLine();

                SaveMatchResult(currentResult);
            }
            myMatchManager.OnSubmissionEnd -= MyMatchManager_OnSubmissionEnd;

            return lastResult;
        }

        private void MyMatchManager_OnSubmissionEnd(object? sender, (DownloadedSubmission, MeasurementRoundContainer) e)
        {
            var (submission, round) = e;
            if (round.Error != null || round.TotalMilliseconds > 5000)
            {
                myCompletedSubmissions.Add(submission.Data);
            }
        }

        private void DrawResultsTable(MatchContainer results, int totalPlaceCount)
        {
            if (!myCompletedSubmissions.Any()) { return; }

            Console.WriteLine();
            TypeInColor(ConsoleColor.White, () =>
            {
                if (myCompletedSubmissions.Count >= totalPlaceCount)
                {
                    Console.WriteLine(" --- Végeredmény ---");
                }
                else
                {
                    Console.WriteLine(" --- Részeredmények ---");
                }
            });
            var currentPlace = totalPlaceCount - myCompletedSubmissions.Count(x => !x.IsAdmin) + 1;
            var maxNameLength = myCompletedSubmissions.Max(x => x.UserName.Length);

            foreach (var (item, index) in myCompletedSubmissions
                .Select(x => (Submission: x, Result: results.Measurements.First(m => m.SubmissionId == x.Id).Result))
                .OrderByDescending(x => x.Result?.DifficultyLevel ?? -1)
                .ThenBy(x => x.Result?.TotalMilliseconds ?? 5001)
                .WithIndex())
            {
                var (submission, result) = item;
                string resultString;
                if (result != null)
                {
                    resultString = $"lvl {result.DifficultyLevel}, {result.TotalMilliseconds.ToString("###0.00")} ms";
                }
                else
                {
                    resultString = "DNF";
                }
                var color = submission.IsAdmin ? ConsoleColor.DarkGray : ConsoleColor.White;
                Console.WriteLine();
                TypeInColor(color, () =>
                    Console.WriteLine($" {(submission.IsAdmin ? "  " : $"{currentPlace++}.")} {submission.UserName.PadRight(maxNameLength)} - {resultString}")
                );
            }
        }

        private void DrawTimeDiagram(int level, int Width, IReadOnlyList<(string Label, double Value)> items)
        {
            if (!items.Any()) { return; }
            var values = items.Select(x => x.Value).ToList();
            var Labels = items.Select(x => x.Label).ToList();
            var width = values.Count;
            var max = 5000;
            var min = values.Min();
            var diff = max - min;
            var step = diff / Width;

            var maxLabelWidth = Labels.Max(x => x.Length);
            TypeInColor(ConsoleColor.White, () =>
                Console.WriteLine($"{$"   lvl {level}".PadRight(maxLabelWidth)}  {"0 ms".PadRight(Width - 6)} 5000 ms")
            );
            Console.WriteLine();
            foreach (var (label, value) in items)
            {
                var valueString = value > 100000 ? "hibás eredmény".PadLeft(10) : value.ToString("0.00").PadLeft(7) + " ms";
                Action action = () => Console.WriteLine($"{label.PadRight(maxLabelWidth)}  [{new string('#', (int)(Math.Min(value, max) / max * Width)).PadRight(Width)}]{(value > max ? '*' : ' ')}  {valueString}");
                if (value > max)
                {
                    TypeInColor(ConsoleColor.DarkGray, action);
                }
                else
                {
                    TypeInColor(ConsoleColor.White, action);
                }
            }
        }

        private void TypeInColor(ConsoleColor color, Action action)
        {
            if (myConfig.UseConsoleColors)
            {
                var prevColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                action();
                Console.ForegroundColor = prevColor;
            }
            else
            {
                action();
            }
        }

        private static void SaveMatchResult(MatchContainer matchResult)
        {
            var matchResultFileInfo = new FileInfo("matchResult.json");
            if (matchResultFileInfo.Exists) { matchResultFileInfo.Delete(); }
            File.WriteAllText(matchResultFileInfo.FullName, JsonSerializer.Serialize(matchResult));
        }

        private static bool TryGetFileInfo(string matchFilePath, out FileInfo matchFileInfo)
        {
            matchFileInfo = new FileInfo(matchFilePath);
            if (!matchFileInfo.Exists)
            {
                Console.WriteLine($"Cannot find match file: {matchFileInfo.FullName}");
                return false;
            }
            return true;
        }

        private DirectoryInfo ExtractZipContents(FileInfo matchZipFile)
        {
            var workingDirectory = myFileManager.CreateNewWorkingDirectory();
            ZipFile.ExtractToDirectory(matchZipFile.FullName, workingDirectory.FullName);

            return workingDirectory;
        }

        private async Task<GetValidSubmissionsResult> GetMatchMetadata(DirectoryInfo workingDirectory)
        {
            var matchMetadataFile = new FileInfo(Path.Combine(workingDirectory.FullName, Constants.MatchMetadataFileName));
            using var stream = new FileStream(matchMetadataFile.FullName, FileMode.Open);
            return await JsonSerializer.DeserializeAsync<GetValidSubmissionsResult>(stream);
        }

        private async Task<List<DownloadedSubmission>> SortSubmissionAssemblies(GetValidSubmissionsResult matchMetadata, DirectoryInfo workingDirectory)
        {
            var downloadedSubmissions = new List<DownloadedSubmission>();
            foreach (var submission in matchMetadata.Submissions)
            {
                var assemblyPath = Path.Combine(workingDirectory.FullName, submission.FileName);
                using var stram = new FileStream(assemblyPath, FileMode.Open);
                var sortedFile = await myFileManager.SaveSubmissionAsync(submission.Id, stram, submission.OriginalFileName);
                downloadedSubmissions.Add(new DownloadedSubmission(sortedFile, submission));
            }

            return downloadedSubmissions;
        }

        private List<GetValidSubmissionsResult.Submission> myCompletedSubmissions = new List<GetValidSubmissionsResult.Submission>();

        private readonly IFanControl myFanControl;
        private readonly FileManager myFileManager;
        private readonly MatchManager myMatchManager;
        private readonly HostConfiguration myConfig;
    }
}
