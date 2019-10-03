using evocontest.Runner.Host.Common.Connection;
using evocontest.Runner.Host.Common.Messages;
using evocontest.Runner.Host.Common.Messages.Request;
using evocontest.Runner.Host.Common.Messages.Response;
using evocontest.Runner.Host.Configuration;
using evocontest.Runner.Host.Core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace evocontest.Runner.Host.Workflow.Steps
{
    public sealed class LoadSubmissionStep : IResolvable
    {
        public LoadSubmissionStep(HostConfiguration config)
        {
            myConfig = config;
        }

        public Task<bool> ExecuteAsync(PipeServer pipeServer, FileInfo targetFile)
        {
            var workerDirectory = new DirectoryInfo(myConfig.Directories.Worker);
            var relativePath = FileManager.GetRelativePath(workerDirectory, targetFile);

            return Task.Run(() =>
            {
                pipeServer.SendMessage(new LoadContextMessage(relativePath));
                var response = pipeServer.ReceiveMessage();
                return ResponseToBool(response);
            });
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

        private readonly HostConfiguration myConfig;
    }
}
