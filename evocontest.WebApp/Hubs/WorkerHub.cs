using System;
using System.Collections.Concurrent;
using evocontest.WebApp.Common;
using evocontest.WebApp.Data.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using evocontest.WebApp.Data;
using System.Linq;
using evocontest.WebApp.Common.Hub;
using Microsoft.EntityFrameworkCore;

namespace evocontest.WebApp.Hubs
{
    [Authorize(Roles = Roles.Worker + "," + Roles.Admin)]
    public class WorkerHub : Hub<IWorkerHubClient>, IWorkerHubServer
    {
        public static ConcurrentDictionary<string, string> Users { get; } = new ConcurrentDictionary<string, string>();

        public WorkerHub(ContestDb contestDb, IHubContext<UserHub, IUserHubClient> userHub)
        {
            myDb = contestDb;
            myUserHub = userHub;
        }

        public override async Task OnConnectedAsync()
        {
            Users.TryAdd(Context.ConnectionId, Context.UserIdentifier);
            await base.OnConnectedAsync();

            var waitingSubmissionIds = myDb.Submissions.AsQueryable()
                .Where(x => !x.IsDeleted && !x.IsValid.HasValue && x.ValidationState < ValidationStateEnum.Completed)
                .Select(x => x.Id)
                .ToArray();

            await Clients.Caller.ValidateSubmissions(waitingSubmissionIds);
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

        public async Task UpdateStatus(string submissionId, ValidationStateEnum state, string error)
        {
            var submission = await myDb.Submissions.Include(x => x.User).SingleAsync(x => x.Id == submissionId);
            if (submission == null) { return; }

            bool? isValid = null;
            submission.ValidationState = state;
            if (error != null)
            {
                submission.IsValid = false;
                submission.Error = error;
                isValid = false;
            }
            else if (state == ValidationStateEnum.Completed)
            {
                isValid = true;
                submission.IsValid = isValid;
            }

            await myDb.SaveChangesAsync();

            await Clients.Others.ReceiveMessage($"UpdateStatus ID:{submissionId}, State:{state}, Error:{error}");
            if (UserHub.UserToConnectionId.TryGetValue(submission.User.Id, out var connectionId))
            {
                await myUserHub.Clients.Client(connectionId).UpdateUploadStatus(state, isValid, error);
            }
        }

        private readonly ContestDb myDb;
        private readonly IHubContext<UserHub, IUserHubClient> myUserHub;
    }
}
