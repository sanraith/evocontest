using evorace.Runner.Host.Connection;
using evorace.Runner.Host.Core;
using evorace.WebApp.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace evorace.Runner.Host.Workflow
{
    public class RaceWorkflow : IResolvable
    {
        public RaceWorkflow(WebAppConnector webApp, DownloadSubmissionStep downloadStep)
        {
            myWebApp = webApp;
            myDownloadStep = downloadStep;
        }

        public async Task ExecuteAsync()
        {
            Console.WriteLine("Running race...");
            var submissionResult = await myWebApp.GetValidSubmissionsAsync();
            var downloadedSubmissions = await DownloadSubmissions(submissionResult);
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
