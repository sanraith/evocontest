using evorace.Runner.Host.Connection;
using evorace.Runner.Host.Core;
using evorace.Runner.Host.Extensions;
using evorace.WebApp.Common;
using evorace.WebApp.Common.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace evorace.Runner.Host.Workflow
{
    public class MatchWorkflow : IResolvable
    {
        public MatchWorkflow(WebAppConnector webApp, DownloadSubmissionStep downloadStep)
        {
            myWebApp = webApp;
            myDownloadStep = downloadStep;
        }

        public async Task ExecuteAsync()
        {
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

        private async Task<MatchContainer> RunMatch(List<DownloadedSubmission> downloadedSubmissions)
        {
            var result = new MatchContainer
            {
                Measurements = GenerateDummyResults(downloadedSubmissions)
            };

            return await Task.FromResult(result);
        }

        private List<MeasurementContainer> GenerateDummyResults(List<DownloadedSubmission> downloadedSubmissions)
        {
            var random = new Random();
            var measurements = new List<MeasurementContainer>();
            foreach (var submission in downloadedSubmissions)
            {
                var measurement = new MeasurementContainer
                {
                    SubmissionId = submission.Data.Id,
                    Rounds = new List<MeasurementRoundContainer>()
                };
                var max = random.Next(1, 4);
                for (int i = 0; i < max; i++)
                {
                    var round = new MeasurementRoundContainer
                    {
                        DifficultyLevel = i,
                        TotalMilliseconds = random.Next(10, 501)
                    };
                    measurement.Rounds.Add(round);
                }
                measurements.Add(measurement);
            }
            return measurements;
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
    }
}
