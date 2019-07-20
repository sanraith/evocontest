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

        public Task ValidateSubmissions(params string[] submissionIds)
        {
            Console.WriteLine("Received submission ids:");
            foreach (var id in submissionIds)
            {
                Console.WriteLine(" - " + id);
            }

            return Task.CompletedTask;
        }
    }
}