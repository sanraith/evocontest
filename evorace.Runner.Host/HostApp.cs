using evorace.Runner.Host.Configuration;
using evorace.Runner.Host.Connection;
using evorace.Runner.Host.Core;
using System;
using System.Threading.Tasks;

namespace evorace.Runner.Host
{
    public sealed class HostApp
    {
        static async Task Main(string[] args)
        {
            await new HostApp().Run();
        }

        private async Task Run()
        {
            var config = HostConfiguration.Load();

            await using var webApp = await ConnectToWebApp(config);
            HubClient client = new HubClient(config, webApp, new FileManager(config));

            var hubProxy = await webApp.ConnectToSignalR(client);

            Console.ReadLine();
        }

        private static async Task<WebAppConnector> ConnectToWebApp(HostConfiguration config)
        {
            var connector = new WebAppConnector(config);
            await connector.Login(config.Login.Email, config.Login.Password);

            return connector;
        }
    }
}
