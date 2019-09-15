using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using evorace.WebApp.Common.Hub;
using evorace.WebApp.Core;
using evorace.WebApp.Data;
using evorace.WebApp.Data.Helper;
using evorace.WebApp.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace evorace.WebApp.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : Controller
    {
        public AdminController(ContestDb db, IFileManager fileManager, UserManager<ApplicationUser> userManager,
            IHubContext<WorkerHub, IWorkerHubClient> workerHub)
        {
            myDb = db;
            myFileManager = fileManager;
            myUserManager = userManager;
            myWorkerHub = workerHub;
        }

        public IActionResult Index() => RedirectToAction(nameof(Admin));

        public async Task<IActionResult> Admin()
        {
            var signalRUsers = new ConcurrentDictionary<ApplicationUser, int>();
            foreach (var userId in WorkerHub.Users.Values)
            {
                var user = await myUserManager.FindByIdAsync(userId);
                signalRUsers.AddOrUpdate(user, 1, (_, connCount) => connCount + 1);
            }

            ViewBag.Message = "SignalR clients: " + string.Join(", ", signalRUsers.Select(x => $"{x.Key.Email} ({x.Value})"));
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DoClearAllSubmissions()
        {
            foreach (var sub in myDb.Submissions.Include(x => x.User))
            {
                try
                {
                    myFileManager.DeleteUserSubmission(sub.User, sub.StoredFileName);
                }
                catch (FileNotFoundException)
                {
                    // do not care
                }
            }

            myDb.Submissions.RemoveRange(myDb.Submissions);
            await myDb.SaveChangesAsync();

            return RedirectToAction(nameof(Admin));
        }

        [HttpPost]
        public async Task<IActionResult> DoRunRace()
        {
            await myWorkerHub.Clients.All.RunRace();

            return RedirectToAction(nameof(Admin));
        }

        private readonly ContestDb myDb;
        private readonly IFileManager myFileManager;
        private readonly UserManager<ApplicationUser> myUserManager;
        private readonly IHubContext<WorkerHub, IWorkerHubClient> myWorkerHub;
    }
}