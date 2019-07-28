using evorace.Runner.Host.Connection;
using evorace.Runner.Host.Core;
using System;
using System.Threading.Tasks;

namespace evorace.Runner.Host.Workflow
{
    public sealed class ListeningWorkflow : IDisposable, IResolvable
    {
        public ListeningWorkflow(WebAppConnector webApp, HubClient hubClient)
        {
            myWebApp = webApp;
            myHubClient = hubClient;
        }

        public async Task Start()
        {
            myHubClient.RunRaceReceived += OnRunRaceReceived;
            myWebApp.InitSignalR(myHubClient);
            await myWebApp.StartSignalR();
        }

        public async Task Stop()
        {
            await myWebApp.StopSignalR();
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
        private readonly TaskCompletionSource<bool> myRunRaceReceived = new TaskCompletionSource<bool>();
    }
}
