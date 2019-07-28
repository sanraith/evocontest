using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using evorace.Runner.Common.Connection;
using evorace.Runner.Common.Messages;
using evorace.Runner.Host.Configuration;
using evorace.Runner.Host.Connection;
using evorace.Runner.Host.Core;
using evorace.Runner.Host.Extensions;
using evorace.WebApp.Common;

using RunnerConstants = evorace.Runner.Common.Constants;

namespace evorace.Runner.Host.Workflow
{
    public sealed class ValidationWorkflow : IResolvable
    {
        public ValidationWorkflow(HostConfiguration config, WebAppConnector webApp, Lazy<LoadStep> loadStep)
        {
            myConfig = config;
            myLoadStep = loadStep;
            myServer = webApp.WorkerHubServer ?? throw new ArgumentException(nameof(webApp));
            myPipeServer = null!;
            myWorkerProcess = null!;
        }

        public async Task Execute(string submissionId)
        {
            var targetFile = await myLoadStep.Value.Execute(submissionId);

            Console.WriteLine($"Validating {targetFile.Name}...");

            bool result;
            using (myWorkerProcess = StartWorkerProcess())
            using (myPipeServer = await StartPipeServer())
            {
                result = LoadSubmissionToWorker(targetFile);
                StopWorkerProcess();
            }

            Console.WriteLine("Validation done.");
            await myServer.UpdateStatus(submissionId, ValidationStateEnum.Static, result ? null : "error")
                .WithProgressLog("Sending validation result to server");
        }

        private bool LoadSubmissionToWorker(FileInfo targetFile)
        {
            var workerDirectory = new DirectoryInfo(myConfig.Directories.Worker);
            var relativePath = GetRelativePath(workerDirectory, targetFile);
            myPipeServer.SendMessage(new LoadContextMessage(relativePath));

            var response = myPipeServer.ReceiveMessage();
            return response switch
            {
                OperationSuccessfulMessage _ => true,
                OperationFailedMessage _ => false,
                _ => throw new InvalidOperationException(),
            };
        }

        private void StopWorkerProcess()
        {
            myPipeServer.SendMessage(new TerminateMessage());

            Console.Write("Waiting for worker process to exit... ");
            myWorkerProcess.WaitForExit(5000);

            if (!myWorkerProcess.HasExited)
            {
                Console.Write("Killing it instead... ");
                myWorkerProcess.Kill(entireProcessTree: true);
            }

            Console.WriteLine($"Exit code: {myWorkerProcess.ExitCode}.");
        }

        private Process StartWorkerProcess()
        {
            ProcessStartInfo startInfo = myConfig.WorkerProcessInfo;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            var process = Process.Start(startInfo);

            return process;
        }

        private static async Task<PipeServer> StartPipeServer()
        {
            var pipeServer = new PipeServer(RunnerConstants.PipeName);
            await pipeServer.WaitForConnectionAsync();

            return pipeServer;
        }

        private static string GetRelativePath(DirectoryInfo directoryInfo, FileInfo fileInfo)
        {
            var absoluteFolder = directoryInfo.FullName;
            if (!absoluteFolder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                absoluteFolder += Path.DirectorySeparatorChar;
            }

            var relativeUri = new Uri(absoluteFolder).MakeRelativeUri(new Uri(fileInfo.FullName));
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));

            return relativePath;
        }

        private Process myWorkerProcess;
        private PipeServer myPipeServer;
        private readonly HostConfiguration myConfig;
        private readonly IWorkerHubServer myServer;
        private readonly Lazy<LoadStep> myLoadStep;
    }
}
