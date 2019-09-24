using evocontest.Runner.Common.Connection;
using evocontest.Runner.Common.Messages.Request;
using evocontest.Runner.Common.Utility;
using evocontest.Runner.Host.Configuration;
using evocontest.Runner.Host.Core;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using RunnerConstants = evocontest.Runner.Common.Constants;

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

        public async Task<DisposableValue<PipeServer>> ExecuteAsync()
        {
            myWorkerProcess = StartWorkerProcess();
            myPipeServer = await StartPipeServerAsync();

            return new DisposableValue<PipeServer>(myPipeServer, pipeServer =>
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
