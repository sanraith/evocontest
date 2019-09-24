using System;
using System.Threading.Tasks;
using evocontest.Runner.Common.Connection;
using evocontest.Runner.Common.Messages.Request;
using evocontest.Runner.Common.Messages.Response;
using evocontest.Runner.Host.Configuration;
using evocontest.Runner.Host.Connection;
using evocontest.Runner.Host.Core;
using evocontest.Runner.Host.Extensions;
using evocontest.Runner.Host.Workflow.Steps;
using evocontest.WebApp.Common;
using evocontest.WebApp.Common.Hub;

namespace evocontest.Runner.Host.Workflow
{
    public sealed class ValidationWorkflow : IResolvable
    {
        public ValidationWorkflow(HostConfiguration config, WebAppConnector webApp, DownloadSubmissionStep downloadStep, 
            SetupEnvironmentStep setupEnvironmentStep, StartWorkerProcessStep startWorkerProcessStep, LoadSubmissionStep loadSubmissionStep)
        {
            myConfig = config;
            myDownloadStep = downloadStep;
            mySetupEnvironmentStep = setupEnvironmentStep;
            myStartWorkerProcessStep = startWorkerProcessStep;
            myLoadSubmissionStep = loadSubmissionStep;
            myServer = webApp.WorkerHubServer ?? throw new ArgumentException(nameof(webApp));
            myPipeServer = null!;
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

            using (var disposablePipe = await myStartWorkerProcessStep.ExecuteAsync())
            {
                myPipeServer = disposablePipe.Value;
                // Load assembly
                errorMessage = "Nem sikerült betölteni a szerelvényt.";
                status = ValidationStateEnum.Static;

                await myServer.UpdateStatus(submissionId, status, null);
                success = await TimedTask(loadTimeout, () => myLoadSubmissionStep.ExecuteAsync(myPipeServer, targetFile));

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

            // Validation completed
            status = success ? ValidationStateEnum.Completed : status;
            Console.WriteLine("Validation done.");
            await myServer.UpdateStatus(submissionId, status, success ? null : errorMessage)
                .WithProgressLog("Sending validation result to server");
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

        private PipeServer myPipeServer;
        private readonly HostConfiguration myConfig;
        private readonly DownloadSubmissionStep myDownloadStep;
        private readonly SetupEnvironmentStep mySetupEnvironmentStep;
        private readonly StartWorkerProcessStep myStartWorkerProcessStep;
        private readonly LoadSubmissionStep myLoadSubmissionStep;
        private readonly IWorkerHubServer myServer;
    }
}
