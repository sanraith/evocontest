using evorace.WebApp.Common;
using evorace.WebApp.Common.Hub;
using evorace.WebApp.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace evorace.WebApp.Hubs
{
    public class UserHub : Hub<IUserHubClient>, IUserHubServer
    {
        public static ConcurrentDictionary<string, string> UserIdToConnectionId { get; } = new ConcurrentDictionary<string, string>();

        public UserHub(ContestDb contestDb)
        {
            myDb = contestDb;
        }

        public override async Task OnConnectedAsync()
        {
            string userId = Context.UserIdentifier;
            UserIdToConnectionId.TryAdd(userId, Context.ConnectionId);
            await base.OnConnectedAsync();

            var sub = await myDb.Submissions
                .Where(x => x.User.Id == userId && !x.IsDeleted)
                .OrderBy(x => x.UploadDate)
                .LastOrDefaultAsync();
            if (sub != null)
            {
                await Clients.Caller.UpdateUploadStatus(sub.ValidationState, sub.IsValid, sub.Error);
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            UserIdToConnectionId.TryRemove(Context.UserIdentifier, out var _);
            return base.OnDisconnectedAsync(exception);
        }

        private readonly ContestDb myDb;
    }
}
