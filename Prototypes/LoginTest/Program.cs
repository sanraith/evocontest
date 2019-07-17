using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using evorace.WebApp.Common;

namespace LoginTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .Build();

            var hostUri = new Uri(config["HostUrl"]);
            var loginUri = new Uri(hostUri, Constants.LoginRoute);
            var signalrUri= new Uri(hostUri, Constants.WorkerHubRoute);

            using var connector = new WebAppConnector(loginUri, signalrUri);
            
            Console.WriteLine(loginUri);
            await connector.Login(config["Login.Email"], config["Login.Password"]);

            Console.WriteLine(signalrUri);
            await connector.ConnectToSignalR(new MyClient());

            Console.ReadLine();
        }

        private class MyClient : IWorkerHubClient
        {
            public Task ReceiveMessage(string message)
            {
                Console.WriteLine(message);
                return Task.CompletedTask;
            }
        }
    }
}
