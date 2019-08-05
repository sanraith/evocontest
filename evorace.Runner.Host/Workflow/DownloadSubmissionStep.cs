using System.IO;
using System.Threading.Tasks;
using evorace.Runner.Host.Connection;
using evorace.Runner.Host.Core;
using evorace.Runner.Host.Extensions;

namespace evorace.Runner.Host.Workflow
{
    public sealed class DownloadSubmissionStep : IResolvable
    {
        public DownloadSubmissionStep(WebAppConnector webApp, FileManager fileManager)
        {
            myWebApp = webApp;
            myFileManager = fileManager;
        }

        public async Task<FileInfo> ExecuteAsync(string submissionId)
        {
            using var disposableValue = await myWebApp.DownloadSubmissionAsync(submissionId).WithProgressLog($"Downlading submission {submissionId}");
            var (fileName, downloadStream) = disposableValue.Value;
            var downloadedFileInfo = await myFileManager.SaveSubmissionAsync(submissionId, downloadStream, fileName).WithProgressLog($"Saving submission file {fileName}");

            return downloadedFileInfo;
        }

        private readonly WebAppConnector myWebApp;
        private readonly FileManager myFileManager;
    }
}
