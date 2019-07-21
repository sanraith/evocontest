using evorace.WebApp.Common;
using System;
using System.Threading.Tasks;
using evorace.Runner.Host.Extensions;
using evorace.Runner.Host.Configuration;
using evorace.Runner.Host.Core;
using System.IO;
using System.Linq;

namespace evorace.Runner.Host.Connection
{
    public class HubClient : IWorkerHubClient
    {
        public HubClient(HostConfiguration config, WebAppConnector webApp, FileManager fileManager)
        {
            myConfig = config;
            myWebApp = webApp;
            myFileManager = fileManager;
        }

        public Task ReceiveMessage(string message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }

        public async Task ValidateSubmissions(params string[] submissionIds)
        {
            foreach (var id in submissionIds)
            {
                var fileInfo = await LoadSubmission(id);

                var wf = new ValidationWorkflow(myConfig);
                await wf.Start(fileInfo);
            }
        }

        private async Task<FileInfo> LoadSubmission(string id)
        {
            using var disposableValue = await myWebApp.DownloadSubmission(id).LogProgress($"Downlading submission {id}");
            var (fileName, downloadStream) = disposableValue.Value;
            var sourceFileInfo = await myFileManager.SaveSubmission(id, downloadStream, fileName).LogProgress($"Saving submission file {fileName}");
            var targetFileInfo = LoggerExtensions.LogProgress("Setting up environment", () => SetupEnvironment(sourceFileInfo));

            return targetFileInfo;
        }

        private FileInfo SetupEnvironment(FileInfo sourceFile)
        {
            var sourceDirectory = new DirectoryInfo(myConfig.Directories.SubmissionTemplate);
            var targetDirectory = new DirectoryInfo(myConfig.Directories.Submission);

            // clean target
            if (!targetDirectory.Exists) { targetDirectory.Create(); }
            targetDirectory.GetFiles().ToList().ForEach(f => f.Delete());
            targetDirectory.GetDirectories().ToList().ForEach(d => d.Delete(true));

            // copy template directory
            sourceDirectory.GetFiles().ToList().ForEach(f => f.CopyTo(Path.Combine(targetDirectory.FullName, f.Name), true));

            // copy submission dll
            var targetFile = sourceFile.CopyTo(Path.Combine(targetDirectory.FullName, sourceFile.Name), true);

            return targetFile;
        }

        private readonly HostConfiguration myConfig;
        private readonly WebAppConnector myWebApp;
        private readonly FileManager myFileManager;
    }
}