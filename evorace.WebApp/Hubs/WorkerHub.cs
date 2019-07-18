using System;
using System.Collections.Concurrent;
using evorace.WebApp.Common;
using evorace.WebApp.Data.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace evorace.WebApp.Hubs
{
    [Authorize(Roles = Roles.Worker)]
    public class WorkerHub : Hub<IWorkerHubClient>, IWorkerHubServer
    {
        public static ConcurrentDictionary<string, string> Users = new ConcurrentDictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            Users.TryAdd(Context.ConnectionId, Context.UserIdentifier);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Users.TryRemove(Context.ConnectionId, out var _);
            return base.OnDisconnectedAsync(exception);
        }

        public Task SendMessage(string status)
        {
            return Clients.Others.ReceiveMessage(status);
        }
    }
}
