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
            HostConfiguration config, IFanControl fanControl)
        {
            myWebApp = webApp;
            myDownloadStep = downloadStep;
            mySetupEnvironmentStep = setupEnvironmentStep;
            myStartWorkerProcessStep = startWorkerProcessStep;
            myLoadSubmissionStep = loadSubmissionStep;
            myMeasureSolveStep = measureSolveStep;
            myConfig = config;
            myFanControl = fanControl;
        }

        public async Task ExecuteAsync()
        {
            Console.WriteLine();
            Console.WriteLine("Running match...");

            using (myFanControl.TurnOnTemporarily())
            {
                var submissionResult = await myWebApp.GetValidSubmissionsAsync();
                var downloadedSubmissions = await DownloadSubmissions(submissionResult);
                var matchResults = await RunMatch(downloadedSubmissions);
                await myWebApp.UploadMatchResults(matchResults).WithProgressLog("Uploading match results");
            }
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
            var random = new Random();
            var difficulty = -1;
            var activeSubmissions = downloadedSubmissions.ToList();
            var measurements = activeSubmissions.Select(x => new MeasurementContainer() { SubmissionId = x.Data.Id }).ToList();
            var inputManager = new InputGeneratorManager();

            while (activeSubmissions.Any() && ++difficulty < 16)
            {
                Console.WriteLine($"--- Difficulty: {difficulty} ---");
                Console.WriteLine($"[{DateTime.Now}] Generating inputs...");
                var inputs = inputManager.Generate(difficulty, roundLength); // TODO write into file instead...
                foreach (var submission in activeSubmissions.Shuffle(random).ToList())
                {
                    Console.WriteLine($"[{DateTime.Now}] Solving: {submission.Data.Id}");
                    var measurement = measurements.First(x => x.SubmissionId == submission.Data.Id);

                    var round = await ExecuteRound(submission, inputs, difficulty);
                    measurement.Rounds.Add(round);
                    if (round.TotalMilliseconds > 1000 || round.Error != null)
                    {
                        activeSubmissions.Remove(submission);
                    }
                }
            }

            return new MatchContainer { Measurements = measurements };
        }

        private async Task<MeasurementRoundContainer> ExecuteRound(DownloadedSubmission submission, IEnumerable<GeneratorResult> inputs, int difficultyLevel)
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

                // TODO Warmup
                _ = await TaskHelper.TimedTask(myConfig.SingleSolveTimeoutMillis, () => myMeasureSolveStep.ExecuteAsync(pipeServer, GeneratorResult.Empty));

                // TODO double check time
                MeasurementError? error = null;
                var configs = new List<InputGeneratorConfig>();
                foreach (var (input, index) in inputs.WithIndex())
                {
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
                                if (!isSolutionValid)
                                {
                                    error = new MeasurementError { ErrorType = MeasurementErrorType.InvalidSolution, ErrorMessage = $"Solution differs from expected at: {input.Solution.Zip(solveMsg.Output ?? "", (c1, c2) => c1 == c2).TakeWhile(b => b).Count()}" };
                                }
                                break;
                            case OperationFailedMessage failMsg:
                                error = new MeasurementError { ErrorType = MeasurementErrorType.Unknown, ErrorMessage = failMsg.ErrorMessage };
                                Console.WriteLine($"Failed operation: {failMsg.ErrorMessage}");
                                break;
                            default:
                                throw new InvalidOperationException("Unknown result from worker process!");
                        }
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine($"Solution timed out.");
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
                    if (timeSum > TimeSpan.FromSeconds(10))
                    {
                        break;
                    }
                }

                return new MeasurementRoundContainer { DifficultyLevel = difficultyLevel, TotalMilliseconds = timeSum.TotalMilliseconds, GeneratorConfigs = configs.ToArray(), Error = error };
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

        private readonly WebAppConnector myWebApp;
        private readonly DownloadSubmissionStep myDownloadStep;
        private readonly SetupEnvironmentStep mySetupEnvironmentStep;
        private readonly StartWorkerProcessStep myStartWorkerProcessStep;
        private readonly LoadSubmissionStep myLoadSubmissionStep;
        private readonly MeasureSolveStep myMeasureSolveStep;
        private readonly HostConfiguration myConfig;
        private readonly IFanControl myFanControl;
    }
}
