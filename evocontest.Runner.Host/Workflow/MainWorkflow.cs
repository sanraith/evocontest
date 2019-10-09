using Autofac;
using evocontest.Runner.Host.Common.Utility;
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

                System.Console.WriteLine();
                var webApp = scope.Resolve<WebAppConnector>();
                var loginSuccess = await webApp.LoginAsync(myConfig.Login.Email, myConfig.Login.Password);
                if (!loginSuccess)
                {
                    await ReconnectCountDown();
                    continue;
                }

                var listener = scope.Resolve<ListeningWorkflow>();
                await listener.StartAsync();
                var isRunRaceReceived = await listener.WaitUntilRunRaceReceivedAsync();
                await listener.StopAsync();

                if (isRunRaceReceived)
                {
                    var matcher = scope.Resolve<MatchWorkflow>();
                    await matcher.ExecuteAsync();
                }
                else
                {
                    System.Console.WriteLine("SignalR connection lost.");
                    await ReconnectCountDown();
                    continue;
                }
            }
        }

        private static async Task ReconnectCountDown()
        {
            await ConsoleUtilities.CountDown(60, i => $"Reconnecting in... {i}", "Reconnecting...");
        }

        private readonly ILifetimeScope myContainer;
        private readonly HostConfiguration myConfig;
    }
}
