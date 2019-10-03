﻿using evocontest.Runner.Common.Extensions;
using evocontest.Runner.Common.Generator;
using evocontest.Runner.Host.Common.Messages.Response;
using evocontest.Runner.Host.Common.Utility;
using evocontest.Runner.Host.Configuration;
using evocontest.Runner.Host.Connection;
using evocontest.Runner.Host.Core;
using evocontest.Runner.Host.Extensions;
using evocontest.Runner.Host.Workflow.Steps;
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
            StartWorkerProcessStep startWorkerProcessStep, LoadSubmissionStep loadSubmissionStep, MeasureSolveStep measureSolveStep, HostConfiguration config)
        {
            myWebApp = webApp;
            myDownloadStep = downloadStep;
            mySetupEnvironmentStep = setupEnvironmentStep;
            myStartWorkerProcessStep = startWorkerProcessStep;
            myLoadSubmissionStep = loadSubmissionStep;
            myMeasureSolveStep = measureSolveStep;
            myConfig = config;
        }

        public async Task ExecuteAsync()
        {
            Console.WriteLine();
            Console.WriteLine("Running match...");

            var submissionResult = await myWebApp.GetValidSubmissionsAsync();
            var downloadedSubmissions = await DownloadSubmissions(submissionResult);
            var matchResults = await RunMatch(downloadedSubmissions);
            await myWebApp.UploadMatchResults(matchResults).WithProgressLog("Uploading match results");
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
            var difficulty = 8;
            var activeSubmissions = downloadedSubmissions.ToList();
            var measurements = activeSubmissions.Select(x => new MeasurementContainer() { SubmissionId = x.Data.Id }).ToList();
            var inputManager = new InputGeneratorManager();

            while (activeSubmissions.Any() && ++difficulty < 20)
            {
                Console.WriteLine($"--- Difficulty: {difficulty} ---");
                var inputs = inputManager.Generate(difficulty, roundLength); // TODO write into file instead...
                foreach (var submission in activeSubmissions.Shuffle(random).ToList())
                {
                    Console.WriteLine($"Solving: {submission.Data.Id}");
                    var measurement = measurements.First(x => x.SubmissionId == submission.Data.Id);

                    var round = await ExecuteRound(submission, inputs, difficulty);
                    measurement.Rounds.Add(round);
                    if (round.TotalMilliseconds >= 2000)
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
                var pipeServer = disposablePipe.Value;
                var success = await myLoadSubmissionStep.ExecuteAsync(pipeServer, targetFile);
                // TODO check success
                var timeSum = new TimeSpan();

                // TODO Warmup
                _ = await TaskHelper.TimedTask(myConfig.SingleSolveTimeoutMillis, () => myMeasureSolveStep.ExecuteAsync(pipeServer, GeneratorResult.Empty));

                // TODO double check time
                // TODO check result
                foreach (var input in inputs)
                {
                    var result = await TaskHelper.TimedTask(myConfig.SingleSolveTimeoutMillis, () => myMeasureSolveStep.ExecuteAsync(pipeServer, input));
                    switch (result)
                    {
                        case MeasureSolveResultMessage solveMsg:
                            // TODO handle invalid case 
                            var isSolutionValid = solveMsg.Output == input.Solution;
                            timeSum += solveMsg.Time;
                            break;
                        case OperationFailedMessage failMsg:
                            Console.WriteLine(failMsg.ErrorMessage);
                            timeSum += TimeSpan.FromDays(1);
                            break;
                        default: throw new InvalidOperationException(result.ToString());
                    }
                }
                var round = new MeasurementRoundContainer { DifficultyLevel = difficultyLevel, TotalMilliseconds = timeSum.TotalMilliseconds };

                return round;
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
    }
}
