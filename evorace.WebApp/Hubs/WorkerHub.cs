using System;
using System.Collections.Concurrent;
using evorace.WebApp.Common;
using evorace.WebApp.Data.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using evorace.WebApp.Data;
using System.Linq;

namespace evorace.WebApp.Hubs
{
    [Authorize(Roles = Roles.Worker + "," + Roles.Admin)]
    public class WorkerHub : Hub<IWorkerHubClient>, IWorkerHubServer
    {
        public static ConcurrentDictionary<string, string> Users { get; } = new ConcurrentDictionary<string, string>();

        public WorkerHub(ContestDb contestDb)
        {
            myDb = contestDb;
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
            var submission = await myDb.Submissions.FindAsync(submissionId);
            if (submission == null) { return; }

            submission.ValidationState = state;
            if (error != null)
            {
                submission.IsValid = false;
                submission.Error = error;
            }

            await myDb.SaveChangesAsync();
        }

        private readonly ContestDb myDb;
    }
}
