﻿using Autofac;
using evorace.Runner.Host.Configuration;
using evorace.Runner.Host.Connection;
using evorace.Runner.Host.Core;
using System.Threading.Tasks;

namespace evorace.Runner.Host.Workflow
{
    public sealed class MainWorkflow : IResolvable
    {
        public MainWorkflow(ILifetimeScope container, HostConfiguration config)
        {
            myContainer = container;
            myConfig = config;
        }

        public async Task ExecuteAsync()
        {
            while (true)
            {
                using var scope = myContainer.BeginLifetimeScope();

                var webApp = scope.Resolve<WebAppConnector>();
                await webApp.LoginAsync(myConfig.Login.Email, myConfig.Login.Password);

                var listener = scope.Resolve<ListeningWorkflow>();
                await listener.StartAsync();
                await listener.WaitUntilRunRaceReceivedAsync();
                await listener.StopAsync();

                var racer = scope.Resolve<RaceWorkflow>();
                await racer.ExecuteAsync();
            }
        }

        private readonly ILifetimeScope myContainer;
        private readonly HostConfiguration myConfig;
    }
}
