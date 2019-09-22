using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using evocontest.Runner.Common.Connection;
using evocontest.Runner.Common.Messages;
using evocontest.Runner.Common.Messages.Request;
using evocontest.Runner.Common.Messages.Response;
using evocontest.Runner.Host.Configuration;
using evocontest.Runner.Host.Connection;
using evocontest.Runner.Host.Core;
using evocontest.Runner.Host.Extensions;
using evocontest.WebApp.Common;
using evocontest.WebApp.Common.Hub;
using RunnerConstants = evocontest.Runner.Common.Constants;

namespace evocontest.Runner.Host.Workflow
{
    public sealed class ValidationWorkflow : IResolvable
    {
        public ValidationWorkflow(HostConfiguration config, WebAppConnector webApp, DownloadSubmissionStep downloadStep, SetupEnvironmentStep setupEnvironmentStep)
        {
            myConfig = config;
            myDownloadStep = downloadStep;
            mySetupEnvironmentStep = setupEnvironmentStep;
            myServer = webApp.WorkerHubServer ?? throw new ArgumentException(nameof(webApp));
            myPipeServer = null!;
            myWorkerProcess = null!;
        }

        public async Task ExecuteAsync(string submissionId)
        {
            var sourceFile = await myDownloadStep.ExecuteAsync(submissionId);
            var targetFile = mySetupEnvironmentStep.Execute(sourceFile);

            Console.WriteLine($"Validating {targetFile.Name}...");

            var success = false;
            string errorMessage;
            var status = ValidationStateEnum.File;

            using (myWorkerProcess = StartWorkerProcess())
            using (myPipeServer = await StartPipeServerAsync())
            {
                try
                {
                    // Load assembly
                    errorMessage = "Nem sikerült betölteni a szerelvényt.";
                    status = ValidationStateEnum.Static;
                    await myServer.UpdateStatus(submissionId, status, null);
                    success = LoadSubmissionToWorker(targetFile);

                    // Run unit tests
                    if (success)
                    {
                        status = ValidationStateEnum.UnitTest;
                        await myServer.UpdateStatus(submissionId, status, null);
                        var unitTestResult = RunUnitTestsInWorker();
                        success = unitTestResult.IsAllPassed;
                        errorMessage = $"Helytelen eredmény a következő unit testekre: {string.Join(", ", unitTestResult.FailedTests)}";
                    }
                }
                finally
                {
                    StopWorkerProcess();
                }
            }

            // Validation completed
            status = success ? ValidationStateEnum.Completed : status;
            Console.WriteLine("Validation done.");
            await myServer.UpdateStatus(submissionId, status, success ? null : errorMessage)
                .WithProgressLog("Sending validation result to server");
        }

        private bool LoadSubmissionToWorker(FileInfo targetFile)
        {
            var workerDirectory = new DirectoryInfo(myConfig.Directories.Worker);
            var relativePath = GetRelativePath(workerDirectory, targetFile);
            myPipeServer.SendMessage(new LoadContextMessage(relativePath));
            var response = myPipeServer.ReceiveMessage();

            return ResponseToBool(response);
        }

        private UnitTestResultMessage RunUnitTestsInWorker()
        {
            myPipeServer.SendMessage(new RunUnitTestsMessage());
            var response = myPipeServer.ReceiveMessage();

            return (UnitTestResultMessage)response;
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

        private static async Task<PipeServer> StartPipeServerAsync()
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

        private static bool ResponseToBool(IMessage response)
        {
            return response switch
            {
                OperationSuccessfulMessage _ => true,
                OperationFailedMessage _ => false,
                _ => throw new InvalidOperationException(),
            };
        }

        private Process myWorkerProcess;
        private PipeServer myPipeServer;
        private readonly HostConfiguration myConfig;
        private readonly DownloadSubmissionStep myDownloadStep;
        private readonly SetupEnvironmentStep mySetupEnvironmentStep;
        private readonly IWorkerHubServer myServer;
    }
}
