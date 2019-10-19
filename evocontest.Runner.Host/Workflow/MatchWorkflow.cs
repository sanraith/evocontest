using evocontest.Runner.Host.Common.Utility;
using evocontest.Runner.Host.Configuration;
using evocontest.Runner.Host.Connection;
using evocontest.Runner.Host.Core;
using evocontest.Runner.Host.Extensions;
using evocontest.Runner.Host.Workflow.Steps;
using evocontest.Runner.RaspberryPiUtilities;
using evocontest.WebApp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DownloadedSubmission = evocontest.Runner.Host.Core.MatchManager.DownloadedSubmission;

namespace evocontest.Runner.Host.Workflow
{
    public class MatchWorkflow : IResolvable
    {
        public MatchWorkflow(WebAppConnector webApp, DownloadSubmissionStep downloadStep, HostConfiguration config, IFanControl fanControl,
            FileManager fileManager, MatchManager matchManager)
        {
            myWebApp = webApp;
            myDownloadStep = downloadStep;
            myConfig = config;
            myFanControl = fanControl;
            myFileManager = fileManager;
            myMatchManager = matchManager;
        }

        public async Task ExecuteAsync()
        {
            Console.WriteLine();
            Console.WriteLine("Running match...");
            var submissionResult = await myWebApp.GetValidSubmissionsAsync();
            var downloadedSubmissions = await DownloadSubmissions(submissionResult);

            using (myFanControl.TurnOnTemporarily())
            {
                await ConsoleUtilities.CountDown(myConfig.CoolDownSeconds, i => $"Cooldown... {i}", "Cooldown complete.");
                var matchResults = await myMatchManager.RunMatch(downloadedSubmissions).LastAsync();
                await myWebApp.UploadMatchResults(matchResults).WithProgressLog("Uploading match results");
                myFileManager.CleanTempDirectory();
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

        private readonly WebAppConnector myWebApp;
        private readonly DownloadSubmissionStep myDownloadStep;
        private readonly HostConfiguration myConfig;
        private readonly IFanControl myFanControl;
        private readonly FileManager myFileManager;
        private readonly MatchManager myMatchManager;
    }
}
