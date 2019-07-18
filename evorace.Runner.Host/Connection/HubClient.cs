using evorace.WebApp.Common;
using System;
using System.Threading.Tasks;

namespace evorace.Runner.Host
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