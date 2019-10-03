using evocontest.Runner.Host.Common.Connection;
using evocontest.Runner.Host.Common.Messages.Request;
using evocontest.Runner.Host.Common.Utility;
using evocontest.Runner.Host.Configuration;
using evocontest.Runner.Host.Core;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using RunnerConstants = evocontest.Runner.Host.Common.Constants;

namespace evocontest.Runner.Host.Workflow.Steps
{
    public class StartWorkerProcessStep : IResolvable
    {
        public StartWorkerProcessStep(HostConfiguration config)
        {
            myConfig = config;
            myPipeServer = null!;
            myWorkerProcess = null!;
        }

        /// <summary>
        /// Starts the worker process, and returns a disposable pipe connection. Disposing the connection will also destroy the worker process.
        /// </summary>
        /// <returns>A pipe connection to the worker process.</returns>
        public async Task<DisposableValue<PipeServer>> ExecuteAsync()
        {
            myWorkerProcess = StartWorkerProcess();
            myPipeServer = await StartPipeServerAsync();

            return DisposableValue.Create(myPipeServer, pipeServer =>
            {
                StopWorkerProcess();
                myPipeServer?.Dispose();
                myWorkerProcess?.Dispose();
            });
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

        private HostConfiguration myConfig;
        private PipeServer myPipeServer;
        private Process myWorkerProcess;
    }
}
