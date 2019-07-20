using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using evorace.WebApp.Core;
using evorace.WebApp.Data;
using evorace.WebApp.Data.Helper;
using evorace.WebApp.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace evorace.WebApp.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : Controller
    {
        public AdminController(ContestDb db, IFileManager fileManager)
        {
            myDb = db;
            myFileManager = fileManager;
        }

        public IActionResult Index() => RedirectToAction(nameof(Admin));
        
        public IActionResult Admin()
        {
            ViewBag.Message = "SignalR clients: " + string.Join(", ", WorkerHub.Users.Values);
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

            ViewBag.Message = "Success";

            return View(nameof(Admin));
        }

        private readonly ContestDb myDb;
        private readonly IFileManager myFileManager;
    }
}