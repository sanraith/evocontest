using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using evocontest.WebApp.Common.Hub;
using evocontest.WebApp.Core;
using evocontest.WebApp.Data;
using evocontest.WebApp.Data.Helper;
using evocontest.WebApp.Hubs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace evocontest.WebApp.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : Controller
    {
        public AdminController(ContestDb db, IFileManager fileManager, UserManager<ApplicationUser> userManager,
            IHubContext<WorkerHub, IWorkerHubClient> workerHub, SignInManager<ApplicationUser> signInManager)
        {
            myDb = db;
            myFileManager = fileManager;
            myUserManager = userManager;
            myWorkerHub = workerHub;
            mySignInManager = signInManager;
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
            ViewBag.Users = myDb.Users.ToList().OrderBy(x => x.FullName).Select(x => (Id: x.Id, Name: x.FullName, Email: x.Email)).ToList();
            ViewBag.Submissions = myDb.Submissions.Include(x => x.User).OrderByDescending(x => x.UploadDate).Where(x => !x.IsDeleted).ToList();
            return View();
        }

        public async Task<IActionResult> ImpersonateUser(string targetUserId)
        {
            var currentUserId = (await myUserManager.GetUserAsync(User)).Id;
            var impersonatedUser = await myUserManager.FindByIdAsync(targetUserId);
            var userPrincipal = await mySignInManager.CreateUserPrincipalAsync(impersonatedUser);

            userPrincipal.Identities.First().AddClaim(new Claim("OriginalUserId", currentUserId));
            userPrincipal.Identities.First().AddClaim(new Claim("IsImpersonating", "true"));

            // sign out the current user
            await mySignInManager.SignOutAsync();

            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, userPrincipal); // <-- This has changed from the previous version.

            return RedirectToAction("Index", "Home");
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
        public async Task<IActionResult> DoClearAllMatches()
        {
            myDb.Matches.RemoveRange(myDb.Matches);
            myDb.Measurements.RemoveRange(myDb.Measurements);
            await myDb.SaveChangesAsync();

            return RedirectToAction(nameof(Admin));
        }

        [HttpPost]
        public async Task<IActionResult> DoRunRace()
        {
            await myWorkerHub.Clients.All.RunRace();

            return RedirectToAction(nameof(Admin));
        }

        public async Task<IActionResult> DownloadSubmission(string submissionId)
        {
            var submission = await myDb.Submissions.Where(x => x.Id == submissionId).Include(x => x.User).FirstOrDefaultAsync();
            if (submission == null) { return NotFound(); }

            var fileInfo = myFileManager.GetFileInfo(submission.User, submission.StoredFileName);
            if (!fileInfo.Exists) { return NotFound(); }

            var fileStream = fileInfo.CreateReadStream();
            return File(fileStream, "application/x-msdownload", submission.OriginalFileName);
        }

        private readonly ContestDb myDb;
        private readonly IFileManager myFileManager;
        private readonly UserManager<ApplicationUser> myUserManager;
        private readonly IHubContext<WorkerHub, IWorkerHubClient> myWorkerHub;
        private readonly SignInManager<ApplicationUser> mySignInManager;
    }
}