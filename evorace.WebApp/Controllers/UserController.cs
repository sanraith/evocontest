using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using evorace.WebApp.Core;
using evorace.WebApp.Data;
using evorace.WebApp.ViewModels;
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

        public async Task<IActionResult> Submit()
        {
            var user = await myUserManager.GetUserAsync(HttpContext.User);
            var latestSubmission = Query(user, u => u.Submissions).LastOrDefault(x => !x.IsDeleted);

            var viewModel = new SubmitViewModel
            {
                LatestSubmission = latestSubmission
            };

            return View(viewModel);
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
            var hasActiveSubmission = Query(user, u => u.Submissions).Any(x => !x.IsDeleted);

            if (hasActiveSubmission)
            {
                // User still has an active submission which has to be deleted first.
                return new ContentResult { Content = "Delete previous submission first." };
            }

            var file = Request.Form.Files.SingleOrDefault();
            var timeStamp = DateTime.Now;

            var checkResult = myFileManager.CheckUserSubmission(file);
            if (checkResult == FileManager.SubmissionFileCheckResult.Ok)
            {
                FileInfo savedFile = null;
                try
                {
                    savedFile = await SaveUserSubmission(user, file, timeStamp);

                    return new ContentResult { Content = "ok" };
                }
                catch (Exception)
                {
                    if (savedFile != null)
                    {
                        myFileManager.DeleteUserSubmission(user, savedFile.Name);
                    }
                }
            }

            return new ContentResult { Content = "error" };
        }

        public async Task<ActionResult> DoDelete(string submissionId)
        {
            var user = await myUserManager.GetUserAsync(HttpContext.User);
            var submissionForDelete = Query(user, u => u.Submissions).LastOrDefault(x => x.Id == submissionId);

            if (submissionForDelete != null)
            {
                submissionForDelete.IsDeleted = true;
                await myDb.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Submit));
        }

        private async Task<FileInfo> SaveUserSubmission(ApplicationUser user, Microsoft.AspNetCore.Http.IFormFile file, DateTime timeStamp)
        {
            var savedFile = await myFileManager.SaveUserSubmissionAsync(user, file, timeStamp);
            var submission = new Submission
            {
                User = user,
                OriginalFileName = file.FileName,
                StoredFileName = savedFile.Name,
                FileSize = (int)savedFile.Length,
                UploadDate = timeStamp,
                ValidationState = Submission.ValidationStateEnum.File,
                IsValid = true
            };

            myDb.Submissions.Add(submission);
            myDb.SaveChanges();

            return savedFile;
        }

        private IQueryable<TProperty> Query<TEntity, TProperty>(TEntity entity,
            Expression<Func<TEntity, IEnumerable<TProperty>>> expression)
            where TEntity : class
            where TProperty : class
        {
            return myDb.Entry(entity).Collection(expression).Query();
        }

        private ContestDb myDb;
        private IFileManager myFileManager;
        private UserManager<ApplicationUser> myUserManager;
    }
}