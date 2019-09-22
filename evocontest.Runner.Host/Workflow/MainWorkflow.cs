using Autofac;
using evocontest.Runner.Host.Configuration;
using evocontest.Runner.Host.Connection;
using evocontest.Runner.Host.Core;
using System.Threading.Tasks;

namespace evocontest.Runner.Host.Workflow
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

                var matcher = scope.Resolve<MatchWorkflow>();
                await matcher.ExecuteAsync();
            }
        }

        private readonly ILifetimeScope myContainer;
        private readonly HostConfiguration myConfig;
    }
}
