using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using evorace.Runner.Common;
using evorace.Runner.Common.Connection;
using evorace.Runner.Common.Messages;
using evorace.Runner.Host.Configuration;

namespace evorace.Runner.Host.Core
{
    public sealed class ValidationWorkflow
    {
        public ValidationWorkflow(HostConfiguration config)
        {
            myConfig = config;
            myPipeServer = null!;
            myWorkerProcess = null!;
        }

        public async Task Execute(FileInfo targetFile)
        {
            Console.WriteLine($"Validating {targetFile.Name}...");

            using (myWorkerProcess = StartWorkerProcess())
            using (myPipeServer = await StartPipeServer())
            {
                LoadSubmissionToWorker(targetFile);
                StopWorkerProcess();
            }

            Console.WriteLine("Validation done.");
        }

        private bool LoadSubmissionToWorker(FileInfo targetFile)
        {
            var workerDirectory = new DirectoryInfo(myConfig.Directories.Worker);
            var relativePath = GetRelativePath(workerDirectory, targetFile);
            myPipeServer.SendMessage(new LoadContextMessage(relativePath));

            var response = myPipeServer.ReceiveMessage();
            switch (response)
            {
                case OperationSuccessfulMessage _: return true;
                case OperationFailedMessage _: return false;
                default: throw new InvalidOperationException();
            }
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
            var pipeServer = new PipeServer(Constants.PipeName);
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
    }
}
