using evocontest.Runner.Common.Extensions;
using evocontest.Runner.Common.Generator;
using evocontest.Runner.Host.Common.Messages;
using evocontest.Runner.Host.Common.Messages.Response;
using evocontest.Runner.Host.Common.Utility;
using evocontest.Runner.Host.Configuration;
using evocontest.Runner.Host.Connection;
using evocontest.Runner.Host.Core;
using evocontest.Runner.Host.Extensions;
using evocontest.Runner.Host.Workflow.Steps;
using evocontest.Runner.RaspberryPiUtilities;
using evocontest.WebApp.Common;
using evocontest.WebApp.Common.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace evocontest.Runner.Host.Workflow
{
    public class MatchWorkflow : IResolvable
    {
        public MatchWorkflow(WebAppConnector webApp, DownloadSubmissionStep downloadStep, SetupEnvironmentStep setupEnvironmentStep,
            StartWorkerProcessStep startWorkerProcessStep, LoadSubmissionStep loadSubmissionStep, MeasureSolveStep measureSolveStep,
            HostConfiguration config, IFanControl fanControl, FileManager fileManager)
        {
            myWebApp = webApp;
            myDownloadStep = downloadStep;
            mySetupEnvironmentStep = setupEnvironmentStep;
            myStartWorkerProcessStep = startWorkerProcessStep;
            myLoadSubmissionStep = loadSubmissionStep;
            myMeasureSolveStep = measureSolveStep;
            myConfig = config;
            myFanControl = fanControl;
            myFileManager = fileManager;
            myMaxRoundMillis = myConfig.MaxRoundSolutionTimeMillis;
        }

        public async Task ExecuteAsync()
        {
            Console.WriteLine();

            using (myFanControl.TurnOnTemporarily())
            {
                await CoolDown();

                Console.WriteLine("Running match...");
                var submissionResult = await myWebApp.GetValidSubmissionsAsync();
                var downloadedSubmissions = await DownloadSubmissions(submissionResult);
                var matchResults = await RunMatch(downloadedSubmissions);
                await myWebApp.UploadMatchResults(matchResults).WithProgressLog("Uploading match results");
                myFileManager.CleanTempDirectory();
            }
        }

        private async Task CoolDown()
        {
            var countDownSeconds = myConfig.CoolDownSeconds;
            for (int i = countDownSeconds; i > 0; i--)
            {
                Console.Write($"Cooldown... {i}  \r");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            Console.WriteLine("Cooldown complete.");
        }

        private async Task<List<DownloadedSubmission>> DownloadSubmissions(GetValidSubmissionsResult submissionsResult)
        {
            var downloadedSubmissions = new List<DownloadedSubmission>();
            foreach (var submission in submissionsResult.Submissions)
            {
                var fileInfo = await myDownloadStep.ExecuteAsync(submission.Id);
                var downloadedSubmission = new DownloadedSubmission(fileInfo, submission);
                downloadedSubmissions.Add(downloadedSubmission);
            }

            return downloadedSubmissions;
        }

        private async Task<MatchContainer> RunMatch(IEnumerable<DownloadedSubmission> downloadedSubmissions)
        {
            const int roundLength = 20;
            const int difficultyCount = 16;
            var random = new Random();
            var difficulty = -1;
            var activeSubmissions = downloadedSubmissions.ToList();
            var measurements = activeSubmissions.Select(x => new MeasurementContainer() { SubmissionId = x.Data.Id }).ToList();

            var seed = random.Next();
            InputGeneratorManager inputGeneratorManger = new InputGeneratorManager(seed);
            var warmupChallenge = inputGeneratorManger.Generate(0, 1).Single();
            var testDataManager = new TestDataManager(seed, inputGeneratorManger);

            while (activeSubmissions.Any() && ++difficulty < difficultyCount)
            {
                Console.WriteLine($"--- Difficulty: {difficulty} ---");
                Console.WriteLine($"[{DateTime.Now}] Generating inputs...");

                // Generate test data up to roundLength
                _ = testDataManager.GetTestData(difficulty, roundLength - 1);

                foreach (var submission in activeSubmissions.Shuffle(random).ToList())
                {
                    Console.WriteLine($"[{DateTime.Now}] Solving: {submission.Data.Id}");
                    var measurement = measurements.First(x => x.SubmissionId == submission.Data.Id);

                    var round = await ExecuteRound(testDataManager, warmupChallenge, submission, difficulty, roundLength);
                    measurement.Rounds.Add(round);
                    if (round.TotalMilliseconds > myMaxRoundMillis || round.Error != null)
                    {
                        activeSubmissions.Remove(submission);
                    }
                }
            }

            testDataManager.Clean();
            return new MatchContainer { Measurements = measurements };
        }

        private async Task<MeasurementRoundContainer> ExecuteRound(TestDataManager testDataManager, GeneratorResult warmupChallenge, DownloadedSubmission submission, int difficultyLevel, int roundLength)
        {
            var targetFile = mySetupEnvironmentStep.Execute(submission.FileInfo);
            using (var disposablePipe = await myStartWorkerProcessStep.ExecuteAsync())
            {
                var round = new MeasurementRoundContainer { DifficultyLevel = difficultyLevel };
                var pipeServer = disposablePipe.Value;
                var success = await myLoadSubmissionStep.ExecuteAsync(pipeServer, targetFile);
                if (!success)
                {
                    round.Error = new MeasurementError { ErrorType = MeasurementErrorType.CouldNotLoadAssembly };
                    return round;
                }

                var timeSum = new TimeSpan();

                // Warmup
                try
                {
                    _ = await TaskHelper.TimedTask(myConfig.WarmupTimeoutMillis, () => myMeasureSolveStep.ExecuteAsync(pipeServer, warmupChallenge));
                }
                catch (TimeoutException)
                {
                    round.Error = new MeasurementError { ErrorType = MeasurementErrorType.Timeout };
                    return round;
                }

                // TODO double check time
                MeasurementError? error = null;
                var configs = new List<InputGeneratorConfig>();
                var splitMilliseconds = new List<double>();
                for (int index = 0; index < roundLength; index++)
                {
                    var input = testDataManager.GetTestData(difficultyLevel, index);
                    configs.Add(input.Config);
                    IMessage? result = null;
                    try
                    {
                        result = await TaskHelper.TimedTask(myConfig.SingleSolveTimeoutMillis, () => myMeasureSolveStep.ExecuteAsync(pipeServer, input));
                        switch (result)
                        {
                            case MeasureSolveResultMessage solveMsg:
                                var isSolutionValid = solveMsg.Output == input.Solution;
                                timeSum += solveMsg.Time;
                                splitMilliseconds.Add(solveMsg.Time.TotalMilliseconds);
                                if (!isSolutionValid)
                                {
                                    error = new MeasurementError { ErrorType = MeasurementErrorType.InvalidSolution, ErrorMessage = $"Hibás megoldás: {input.Solution.Zip(solveMsg.Output ?? "", (c1, c2) => c1 == c2).TakeWhile(b => b).Count()}" };
                                }
                                break;
                            case OperationFailedMessage failMsg:
                                error = new MeasurementError { ErrorType = MeasurementErrorType.Unknown, ErrorMessage = failMsg.ErrorMessage };
                                Console.WriteLine($"Ismeretlen hiba: {failMsg.ErrorMessage}");
                                break;
                            default:
                                throw new InvalidOperationException("Impossible case happened. :(");
                        }
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine($"Single solution timeout exceeded.");
                        error = new MeasurementError { ErrorType = MeasurementErrorType.Timeout };
                    }
                    catch (Exception ex)
                    {
                        error = new MeasurementError { ErrorType = MeasurementErrorType.Unknown, ErrorMessage = ex.Message };
                    }

                    if (error != null)
                    {
                        timeSum += TimeSpan.FromHours(1);
                        break;
                    }
                    if (timeSum > TimeSpan.FromMilliseconds(myMaxRoundMillis))
                    {
                        Console.WriteLine("Max solution time exceeded.");
                        break;
                    }
                }

                Console.WriteLine($"Round completion time: {timeSum.TotalMilliseconds} ms.");

                return new MeasurementRoundContainer
                {
                    DifficultyLevel = difficultyLevel,
                    TotalMilliseconds = timeSum.TotalMilliseconds,
                    SplitMilliseconds = splitMilliseconds.ToArray(),
                    GeneratorConfigs = configs.ToArray(),
                    Error = error
                };
            }
        }

        private sealed class DownloadedSubmission
        {
            public FileInfo FileInfo { get; }

            public GetValidSubmissionsResult.Submission Data { get; }

            public DownloadedSubmission(FileInfo fileInfo, GetValidSubmissionsResult.Submission data)
            {
                FileInfo = fileInfo;
                Data = data;
            }
        }

        private readonly int myMaxRoundMillis;
        private readonly WebAppConnector myWebApp;
        private readonly DownloadSubmissionStep myDownloadStep;
        private readonly SetupEnvironmentStep mySetupEnvironmentStep;
        private readonly StartWorkerProcessStep myStartWorkerProcessStep;
        private readonly LoadSubmissionStep myLoadSubmissionStep;
        private readonly MeasureSolveStep myMeasureSolveStep;
        private readonly HostConfiguration myConfig;
        private readonly IFanControl myFanControl;
        private readonly FileManager myFileManager;
    }
}
