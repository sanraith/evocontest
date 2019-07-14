using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using evorace.WebApp.Core;
using evorace.WebApp.Data;
using evorace.WebApp.Data.Helper;
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

        public IActionResult Admin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DoClearAllSubmissions()
        {
            foreach (var sub in myDb.Submissions.Include(x => x.User))
            {
                myFileManager.DeleteUserSubmission(sub.User, sub.StoredFileName);
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