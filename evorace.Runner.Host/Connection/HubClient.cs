using evorace.WebApp.Common;
using System;
using System.Threading.Tasks;
using evorace.Runner.Host.Workflow;
using evorace.Runner.Host.Core;

namespace evorace.Runner.Host.Connection
{
    public class HubClient : IWorkerHubClient, IResolvable
    {
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
            foreach (var submissionId in submissionIds)
            {
                var workflow = myValidationWorkflow.Value;
                await workflow.Execute(submissionId);
            }
        }

        private readonly Lazy<ValidationWorkflow> myValidationWorkflow;
    }
}