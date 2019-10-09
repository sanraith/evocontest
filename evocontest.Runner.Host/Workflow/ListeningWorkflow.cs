using evocontest.Runner.Host.Connection;
using evocontest.Runner.Host.Core;
using System;
using System.Threading.Tasks;

namespace evocontest.Runner.Host.Workflow
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
            myWebApp.SignalRConnectionLost += OnSignalRConnectionLost;
            myWebApp.InitSignalR(myHubClient);
            await myWebApp.StartSignalRAsync();
        }

        public async Task StopAsync()
        {
            await myValidationJobHandler.StopAsync();
            await myWebApp.StopSignalRAsync();
        }

        public Task<bool> WaitUntilRunRaceReceivedAsync()
        {
            return myRunRaceReceived.Task;
        }

        public void Dispose()
        {
            myHubClient.RunRaceReceived -= OnRunRaceReceived;
            myWebApp.SignalRConnectionLost -= OnSignalRConnectionLost;
        }

        private void OnRunRaceReceived(object? sender, EventArgs? args)
        {
            myRunRaceReceived.TrySetResult(true);
        }

        private void OnSignalRConnectionLost(object? sender, Exception exception)
        {
            myRunRaceReceived.TrySetResult(false);
        }

        private readonly HubClient myHubClient;
        private readonly WebAppConnector myWebApp;
        private readonly ValidationJobHandler myValidationJobHandler;
        private readonly TaskCompletionSource<bool> myRunRaceReceived = new TaskCompletionSource<bool>();
    }
}
