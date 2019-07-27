using evorace.WebApp.Common;
using System;
using System.Threading.Tasks;
using evorace.Runner.Host.Extensions;
using evorace.Runner.Host.Configuration;
using evorace.Runner.Host.Core;
using System.IO;
using System.Linq;

namespace evorace.Runner.Host.Connection
{
    public class HubClient : IWorkerHubClient
    {
        public IWorkerHubServer? WorkerHubServer { get; set; }

        public HubClient(HostConfiguration config, WebAppConnector webApp, FileManager fileManager)
        {
            myConfig = config;
            myWebApp = webApp;
            myFileManager = fileManager;
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
                var workflow = new ValidationWorkflow(myConfig, 
                    WorkerHubServer ?? throw new ArgumentNullException(nameof(WorkerHubServer)),
                    myWebApp, myFileManager);
                await workflow.Execute(submissionId);
            }
        }

        private readonly HostConfiguration myConfig;
        private readonly WebAppConnector myWebApp;
        private readonly FileManager myFileManager;
    }
}