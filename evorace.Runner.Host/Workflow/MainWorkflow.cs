using Autofac;
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

        public async Task Execute()
        {
            while (true)
            {
                using var scope = myContainer.BeginLifetimeScope();

                var webApp = scope.Resolve<WebAppConnector>();
                await webApp.Login(myConfig.Login.Email, myConfig.Login.Password);

                var listener = scope.Resolve<ListeningWorkflow>();
                await listener.Start();
                await listener.WaitUntilRunRaceReceivedAsync();
                await listener.Stop();

                var racer = scope.Resolve<RaceWorkflow>();
                await racer.Execute();
            }
        }

        private readonly ILifetimeScope myContainer;
        private readonly HostConfiguration myConfig;
    }
}
