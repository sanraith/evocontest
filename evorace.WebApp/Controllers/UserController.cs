using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using evorace.WebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace evorace.WebApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        public UserController(UserManager<IdentityUser> userManager, ContestDb database)
        {
            myUserManager = userManager;
            myDb = database;
        }

        public IActionResult Submit()
        {
            return View();
        }

        public IActionResult Stats()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DoUpload()
        {
            var user = await myUserManager.GetUserAsync(HttpContext.User);
            var submission = new Submission
            {
                User = user,
                OriginalFileName = "tempOriginal",
                StoredFileName = "tempStored",
                FileSize = 1_048_576,
                UploadDate = DateTime.Now,
                IsValid = true,
                IsDeleted = false
            };
            myDb.Submissions.Add(submission);
            myDb.SaveChanges();

            return new ContentResult() { Content = "ok" };
        }


        private UserManager<IdentityUser> myUserManager;
        private ContestDb myDb;
    }
}