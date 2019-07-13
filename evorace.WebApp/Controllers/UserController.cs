using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using evorace.WebApp.Core;
using evorace.WebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace evorace.WebApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        public UserController(UserManager<ApplicationUser> userManager, IFileManager fileManager, ContestDb database)
        {
            myUserManager = userManager;
            myFileManager = fileManager;
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
        [RequestSizeLimit(FileManager.MaxSubmittedFileSize + 4096)]
        public async Task<IActionResult> DoUpload()
        {
            var user = await myUserManager.GetUserAsync(HttpContext.User);
            var file = Request.Form.Files.SingleOrDefault();

            var checkResult = myFileManager.CheckUserSubmission(file);
            if (checkResult != FileManager.SubmissionFileCheckResult.Ok)
            {
                return new ContentResult { Content = "Error: " + checkResult.ToString() };
            }

            try
            {
                var timeStamp = DateTime.Now;
                var savedFile = await myFileManager.SaveUserSubmissionAsync(user, file, timeStamp);
                var submission = new Submission
                {
                    User = user,
                    OriginalFileName = file.Name,
                    StoredFileName = savedFile.Name,
                    FileSize = (int)savedFile.Length,
                    UploadDate = timeStamp,
                    IsValid = true,
                    IsDeleted = false
                };
                myDb.Submissions.Add(submission);
                myDb.SaveChanges();

                return new ContentResult { Content = "ok" };
            }
            catch (Exception)
            {
                // TODO
            }

            return new ContentResult { Content = "error" };
        }

        private UserManager<ApplicationUser> myUserManager;
        private IFileManager myFileManager;
        private ContestDb myDb;
    }
}