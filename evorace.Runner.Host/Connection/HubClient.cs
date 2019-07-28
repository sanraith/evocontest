using evorace.WebApp.Common;
using System;
using System.Threading.Tasks;
using evorace.Runner.Host.Workflow;
using evorace.Runner.Host.Core;

namespace evorace.Runner.Host.Connection
{
    public class HubClient : IWorkerHubClient, IResolvable
    {
        public event EventHandler RunRaceReceived;

        public HubClient(Lazy<ValidationWorkflow> validationWorkflow)
        {
            myValidationWorkflow = validationWorkflow;
        }

        public Task ReceiveMessage(string message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }

        public async Task ValidateSubmissions(params string[] submissionIds)
        {
            var workflow = myValidationWorkflow.Value;
            foreach (var submissionId in submissionIds)
            {
                await workflow.Execute(submissionId);
            }
        }

        public Task RunRace()
        {
            RunRaceReceived?.Invoke(this, new EventArgs());
            return Task.CompletedTask;
        }

        private readonly Lazy<ValidationWorkflow> myValidationWorkflow;
    }
}