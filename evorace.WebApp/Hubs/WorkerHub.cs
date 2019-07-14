using evorace.WebApp.Common;
using evorace.WebApp.Data.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace evorace.WebApp.Hubs
{
    [Authorize(Roles = Roles.Worker)]
    public class WorkerHub : Hub<IWorkerHubClient>
    {
        public Task SendMessage(string message)
        {
            return Clients.All.ReceiveMessage(message);
        }
    }
}
