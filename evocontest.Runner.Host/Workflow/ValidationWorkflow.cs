using System;
using System.Threading.Tasks;
using evocontest.Runner.Host.Common.Connection;
using evocontest.Runner.Host.Common.Messages.Request;
using evocontest.Runner.Host.Common.Messages.Response;
using evocontest.Runner.Host.Common.Utility;
using evocontest.Runner.Host.Configuration;
using evocontest.Runner.Host.Connection;
using evocontest.Runner.Host.Core;
using evocontest.Runner.Host.Extensions;
using evocontest.Runner.Host.Workflow.Steps;
using evocontest.Runner.RaspberryPiUtilities;
using evocontest.WebApp.Common;
using evocontest.WebApp.Common.Hub;

namespace evocontest.Runner.Host.Workflow
{
    public sealed class ValidationWorkflow : IResolvable
    {
        public ValidationWorkflow(HostConfiguration config, WebAppConnector webApp, DownloadSubmissionStep downloadStep,
            SetupEnvironmentStep setupEnvironmentStep, StartWorkerProcessStep startWorkerProcessStep,
            LoadSubmissionStep loadSubmissionStep, IFanControl fanControl)
        {
            myConfig = config;
            myDownloadStep = downloadStep;
            mySetupEnvironmentStep = setupEnvironmentStep;
            myStartWorkerProcessStep = startWorkerProcessStep;
            myLoadSubmissionStep = loadSubmissionStep;
            myFanControl = fanControl;
            myServer = webApp.WorkerHubServer ?? throw new ArgumentException(nameof(webApp));
            myPipeServer = null!;
        }

        public async Task ExecuteAsync(string submissionId)
        {
            Console.WriteLine();
            using (myFanControl.TurnOnTemporarily())
            {
                await Validate(submissionId);
            }
        }

        private async Task Validate(string submissionId)
        {
            var loadTimeout = TimeSpan.FromSeconds(5);
            var unitTestTimeout = TimeSpan.FromSeconds(30);

            var sourceFile = await myDownloadStep.ExecuteAsync(submissionId);
            var targetFile = mySetupEnvironmentStep.Execute(sourceFile);

            Console.WriteLine($"[{DateTime.Now}] Validating {targetFile.Name}...");

            var success = false;
            string errorMessage;
            var status = ValidationStateEnum.File;

            try
            {
                using (var disposablePipe = await myStartWorkerProcessStep.ExecuteAsync())
                {
                    myPipeServer = disposablePipe.Value;
                    // Load assembly
                    errorMessage = "Nem sikerült betölteni a szerelvényt.";
                    status = ValidationStateEnum.Static;

                    await myServer.UpdateStatus(submissionId, status, null);
                    success = await TaskHelper.TimedTask(loadTimeout, () => myLoadSubmissionStep.ExecuteAsync(myPipeServer, targetFile));

                    // Run unit tests
                    if (success)
                    {
                        status = ValidationStateEnum.UnitTest;
                        await myServer.UpdateStatus(submissionId, status, null);
                        var unitTestResult = await TaskHelper.TimedTask(unitTestTimeout, RunUnitTestsInWorker);
                        success = unitTestResult.IsAllPassed;
                        errorMessage = $"Helytelen eredmény a következő unit testekre: {string.Join(", ", unitTestResult.FailedTests)}";
                    }
                }
            }
            catch (TimeoutException)
            {
                success = false;
                errorMessage = "A szerelvényt nem lehetett ellenőrizni a megadott időn belül.";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                success = false;
                errorMessage = "Ismeretlen hiba történt az ellenőrzés során.";
            }

            // Validation completed
            status = success ? ValidationStateEnum.Completed : status;
            Console.WriteLine($"Validation done. {status}, Success: {success}{(success ? "" : ", Error: " + errorMessage)}");
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

        private PipeServer myPipeServer;
        private readonly HostConfiguration myConfig;
        private readonly DownloadSubmissionStep myDownloadStep;
        private readonly SetupEnvironmentStep mySetupEnvironmentStep;
        private readonly StartWorkerProcessStep myStartWorkerProcessStep;
        private readonly LoadSubmissionStep myLoadSubmissionStep;
        private readonly IFanControl myFanControl;
        private readonly IWorkerHubServer myServer;
    }
}
