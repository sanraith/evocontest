using evorace.WebApp.Common;
using System;
using System.Threading.Tasks;
using evorace.Runner.Host.Core;
using evorace.Runner.Host.Workflow;

namespace evorace.Runner.Host.Connection
{
    public class HubClient : IWorkerHubClient, IResolvable
    {
        public event EventHandler RunRaceReceived;

        public HubClient(ValidationJobHandler validationJobQueue)
        {
            myValidationJobQueue = validationJobQueue;
            myValidationJobQueue.Start();
        }

        public Task ReceiveMessage(string message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }

        public Task ValidateSubmissions(params string[] submissionIds)
        {
            myValidationJobQueue.Enqueue(submissionIds);
            return Task.CompletedTask;
        }

        public async Task RunRace()
        {
            await myValidationJobQueue.Stop();
            RunRaceReceived?.Invoke(this, new EventArgs());
        }

        private readonly ValidationJobHandler myValidationJobQueue;
    }
}