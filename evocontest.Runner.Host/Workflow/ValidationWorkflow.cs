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
            var loadTimeout = TimeSpan.FromSeconds(5);
            var unitTestTimeout = TimeSpan.FromSeconds(30);
            
            Console.WriteLine();
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
                    success = await TimedTask(loadTimeout, () => LoadSubmissionToWorker(targetFile));

                    // Run unit tests
                    if (success)
                    {
                        status = ValidationStateEnum.UnitTest;
                        await myServer.UpdateStatus(submissionId, status, null);
                        var unitTestResult = await TimedTask(unitTestTimeout, RunUnitTestsInWorker);
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

        private Task<bool> LoadSubmissionToWorker(FileInfo targetFile)
        {
            var workerDirectory = new DirectoryInfo(myConfig.Directories.Worker);
            var relativePath = GetRelativePath(workerDirectory, targetFile);

            return Task.Run(() =>
            {
                myPipeServer.SendMessage(new LoadContextMessage(relativePath));
                var response = myPipeServer.ReceiveMessage();
                return ResponseToBool(response);
            });
        }

        private Task<UnitTestResultMessage> RunUnitTestsInWorker()
        {
            return Task.Run(() =>
            {
                myPipeServer.SendMessage(new RunUnitTestsMessage());
                var response = myPipeServer.ReceiveMessage();
                return (UnitTestResultMessage)response;
            });
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

        private static Task TimedTask(TimeSpan timeout, Func<Task> taskFunc)
        {
            return TimedTask(timeout, () => taskFunc().ContinueWith(_ => true));
        }

        private static async Task<TResult> TimedTask<TResult>(TimeSpan timeout, Func<Task<TResult>> workTaskGenerator)
        {
            Exception? disqualifyException = null;
            Task<TResult> workTask;
            Task timerTask;

            try
            {
                timerTask = Task.Delay(timeout);
                workTask = workTaskGenerator();
                var completedTask = await Task.WhenAny(timerTask, workTask);
                var success = workTask.Equals(completedTask);
                if (success)
                {
                    return workTask.Result;
                }
            }
            catch (Exception ex)
            {
                disqualifyException = ex;
            }
            finally
            {
                // TODO cancel task?
            }

            throw disqualifyException ?? new TimeoutException();
        }

        private Process myWorkerProcess;
        private PipeServer myPipeServer;
        private readonly HostConfiguration myConfig;
        private readonly DownloadSubmissionStep myDownloadStep;
        private readonly SetupEnvironmentStep mySetupEnvironmentStep;
        private readonly IWorkerHubServer myServer;
    }
}
