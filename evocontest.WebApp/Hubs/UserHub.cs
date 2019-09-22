using evocontest.WebApp.Common.Hub;
using evocontest.WebApp.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace evocontest.WebApp.Hubs
{
    public class UserHub : Hub<IUserHubClient>, IUserHubServer
    {
        public static ConcurrentDictionary<string, string> UserToConnectionId { get; } = new ConcurrentDictionary<string, string>();

        public UserHub(ContestDb contestDb)
        {
            myDb = contestDb;
        }

        public override async Task OnConnectedAsync()
        {
            string userId = Context.UserIdentifier;
            UserToConnectionId.TryAdd(userId, Context.ConnectionId);
            await base.OnConnectedAsync();

            Submission sub = await GetSubmissionForUserId(userId);
            if (sub != null)
            {
                await Clients.Caller.UpdateUploadStatus(sub.ValidationState, sub.IsValid, sub.Error);
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            UserToConnectionId.TryRemove(Context.UserIdentifier, out var _);
            return base.OnDisconnectedAsync(exception);
        }

        private Task<Submission> GetSubmissionForUserId(string userId)
        {
            return myDb.Submissions
                .Where(x => x.User.Id == userId && !x.IsDeleted)
                .OrderBy(x => x.UploadDate)
                .LastOrDefaultAsync();
        }

        private readonly ContestDb myDb;
    }
}
