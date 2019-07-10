﻿using evorace.Runner.Common.Connection;
using System.Threading.Tasks;

namespace evorace.Runner.Host
{
    public sealed class HostApp
    {
        public const string PipeName = "evorace.Runner";

        static async Task Main(string[] args)
        {
            await new HostApp().Run();
        }

        private async Task Run()
        {
            using var pipeServer = new PipeServer(PipeName);
            // TODO start worker
            await pipeServer.WaitForConnectionAsync();
            // TODO send commands for worker
        }
    }
}