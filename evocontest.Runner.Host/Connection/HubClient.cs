using System;
using System.Threading.Tasks;
using evocontest.Runner.Host.Core;
using evocontest.Runner.Host.Workflow;
using evocontest.WebApp.Common.Hub;

namespace evocontest.Runner.Host.Connection
{
    public class HubClient : IWorkerHubClient, IResolvable
    {
        public event EventHandler? RunRaceReceived;

        public HubClient(ValidationJobHandler validationJobQueue)
        {
            myValidationJobHandler = validationJobQueue;
        }

        public Task ReceiveMessage(string message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }

        public Task ValidateSubmissions(params string[] submissionIds)
        {
            myValidationJobHandler.Enqueue(submissionIds);
            return Task.CompletedTask;
        }

        public Task RunRace()
        {
            RunRaceReceived?.Invoke(this, new EventArgs());
            return Task.CompletedTask;
        }

        private readonly ValidationJobHandler myValidationJobHandler;
    }
}