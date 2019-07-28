using evorace.Runner.Host.Configuration;
using evorace.Runner.Host.Connection;
using evorace.Runner.Host.Core;
using evorace.Runner.Host.Extensions;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace evorace.Runner.Host.Workflow
{
    public sealed class LoadStep : IResolvable
    {
        public LoadStep(HostConfiguration config, WebAppConnector webApp, FileManager fileManager)
        {
            myConfig = config;
            myWebApp = webApp;
            myFileManager = fileManager;
        }

        public async Task<FileInfo> Execute(string submissionId)
        {
            using var disposableValue = await myWebApp.DownloadSubmission(submissionId).WithProgressLog($"Downlading submission {submissionId}");
            var (fileName, downloadStream) = disposableValue.Value;
            var sourceFileInfo = await myFileManager.SaveSubmission(submissionId, downloadStream, fileName).WithProgressLog($"Saving submission file {fileName}");
            var targetFileInfo = LoggerExtensions.WithProgressLog("Setting up environment", () => SetupEnvironment(sourceFileInfo));

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
