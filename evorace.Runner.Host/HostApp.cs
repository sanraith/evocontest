﻿using evorace.Runner.Common.Connection;
using evorace.Runner.Common.Messages;
using evorace.Runner.Host.Configuration;
using evorace.Runner.Host.Connection;
using evorace.WebApp.Common;
using System;
using System.Diagnostics;
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
            var config = RunnerHostConfiguration.Load();

            await using var webApp = await ConnectToWebApp(config);
            var hubProxy = await webApp.ConnectToSignalR(new HubClient());

            var process = Process.Start(config.WorkerProcessInfo);

            using var pipeServer = new PipeServer(PipeName);
            await pipeServer.WaitForConnectionAsync();

            pipeServer.SendMessage(new LoadContextMessage("TODO add assembly name"));
            var response = pipeServer.ReceiveMessage();
            Console.WriteLine(response.ToString());
            pipeServer.SendMessage(new TerminateMessage());

            process.WaitForExit();
            Console.WriteLine("Worker process finished.");

            Console.ReadLine();
        }

        private static async Task<WebAppConnector> ConnectToWebApp(RunnerHostConfiguration config)
        {
            var hostUri = new Uri(config.HostUrl);
            var loginUri = new Uri(hostUri, Constants.LoginRoute);
            var workerHubUri = new Uri(hostUri, Constants.WorkerHubRoute);

            var connector = new WebAppConnector(loginUri, workerHubUri);
            await connector.Login(config.Login.Email, config.Login.Password);

            return connector;
        }
    }
}
