﻿using System;
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
            var status = ValidationStateEnum.File;
            using (myWorkerProcess = StartWorkerProcess())
            using (myPipeServer = await StartPipeServerAsync())
            {
                // Load assembly
                status = ValidationStateEnum.Static;
                success = LoadSubmissionToWorker(targetFile);

                // Run unit tests
                if (success)
                {
                    status = ValidationStateEnum.UnitTest;
                    success = RunUnitTestsInWorker();
                }

                // Validation completed
                if (success)
                {
                    status = ValidationStateEnum.Completed;
                }

                StopWorkerProcess();
            }

            Console.WriteLine("Validation done.");
            await myServer.UpdateStatus(submissionId, status, success ? null : "error")
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

        private bool RunUnitTestsInWorker()
        {
            myPipeServer.SendMessage(new RunUnitTestsMessage());
            var response = myPipeServer.ReceiveMessage();

            return ResponseToBool(response);
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
