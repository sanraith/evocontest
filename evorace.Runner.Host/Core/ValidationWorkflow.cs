using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using evorace.Runner.Common.Connection;
using evorace.Runner.Common.Messages;
using evorace.Runner.Host.Configuration;

namespace evorace.Runner.Host.Core
{
    public sealed class ValidationWorkflow
    {
        public const string PipeName = "evorace.Runner";

        public ValidationWorkflow(HostConfiguration config)
        {
            myConfig = config;
        }

        public async Task Start(FileInfo targetFile)
        {
            // Setup sandbox
            var process = Process.Start(myConfig.WorkerProcessInfo);
            using var pipeServer = new PipeServer(PipeName);
            await pipeServer.WaitForConnectionAsync();

            // Load context to sandbox
            var workerDirectory = new DirectoryInfo(myConfig.Directories.Worker);
            var relativePath = GetRelativePath(workerDirectory, targetFile);
            pipeServer.SendMessage(new LoadContextMessage(relativePath));

            var response = pipeServer.ReceiveMessage();
            switch (response)
            {
                case OperationSuccessfulMessage _: Console.WriteLine("success"); break;
                case OperationFailedMessage _: Console.WriteLine("fail"); break; // TODO handle fail
                default: throw new InvalidOperationException();
            }

            pipeServer.SendMessage(new TerminateMessage());

            process.WaitForExit();
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

        private readonly HostConfiguration myConfig;
    }
}
