using evorace.WebApp.Common;
using System;
using System.Threading.Tasks;

namespace evorace.Runner.Host.Connection
{
    public class HubClient : IWorkerHubClient
    {
        public Task ReceiveMessage(string message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }
    }
}