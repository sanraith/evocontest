﻿using evocontest.Runner.Common.Connection;
using evocontest.Runner.Host.Common;
using evocontest.Runner.Worker.Core;
using System.Threading.Tasks;

namespace evocontest.Runner.Worker
{
    public sealed class WorkerApp
    {
        static async Task Main(string[] args)
        {
            await new WorkerApp().Run();
        }

        public async Task Run()
        {
            using var pipeClient = new PipeClient(Constants.PipeName);
            await pipeClient.ConnectAsync();

            var messageHandler = new MessageHandler();

            var isDone = false;
            while (!isDone)
            {
                var message = pipeClient.ReceiveMessage();
                var result = messageHandler.Handle(message);

                if (result.Response != null)
                {
                    pipeClient.SendMessage(result.Response);
                }

                isDone = result.IsDone;
            }
        }
    }
}
