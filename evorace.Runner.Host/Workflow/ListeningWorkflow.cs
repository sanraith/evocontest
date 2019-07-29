using evorace.Runner.Host.Connection;
using evorace.Runner.Host.Core;
using System;
using System.Threading.Tasks;

namespace evorace.Runner.Host.Workflow
{
    public sealed class ListeningWorkflow : IDisposable, IResolvable
    {
        public ListeningWorkflow(WebAppConnector webApp, HubClient hubClient, ValidationJobHandler validationJobHandler)
        {
            myWebApp = webApp;
            myHubClient = hubClient;
            myValidationJobHandler = validationJobHandler;
        }

        public async Task StartAsync()
        {
            myHubClient.RunRaceReceived += OnRunRaceReceived;
            myWebApp.InitSignalR(myHubClient);
            await myWebApp.StartSignalRAsync();
        }

        public async Task StopAsync()
        {
            await myValidationJobHandler.StopAsync();
            await myWebApp.StopSignalRAsync();
        }

        public Task WaitUntilRunRaceReceivedAsync()
        {
            return myRunRaceReceived.Task;
        }

        public void Dispose()
        {
            myHubClient.RunRaceReceived -= OnRunRaceReceived;
        }

        private void OnRunRaceReceived(object? sender, EventArgs? args)
        {
            myRunRaceReceived.TrySetResult(true);
        }

        private readonly HubClient myHubClient;
        private readonly WebAppConnector myWebApp;
        private readonly ValidationJobHandler myValidationJobHandler;
        private readonly TaskCompletionSource<bool> myRunRaceReceived = new TaskCompletionSource<bool>();
    }
}
