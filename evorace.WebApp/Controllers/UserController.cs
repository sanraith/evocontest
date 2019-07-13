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
using Microsoft.AspNetCore.Http;
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
                return BadRequest(new { success = false, error = "Már van aktív nevezésed. Frissítsd az oldalt." });
            }

            var success = false;
            var file = Request.Form.Files.SingleOrDefault();
            var checkResult = myFileManager.CheckUserSubmission(file);
            if (checkResult == FileManager.SubmissionFileCheckResult.Ok)
            {
                success = await SaveUserSubmission(user, file);
            }

            return success ?
                Ok(new { success }) as IActionResult :
                BadRequest(new
                {
                    success = false,
                    error = checkResult switch
                    {
                        FileManager.SubmissionFileCheckResult.NoFile => "Hiányzó fájl.",
                        FileManager.SubmissionFileCheckResult.InvalidSize => $"Túl nagy méretű fájl. Maximális méret: {FileManager.MaxSubmittedFileSize / 1024 / 1024} MB.",
                        FileManager.SubmissionFileCheckResult.InvalidFileExtension => "Hibás kiterjesztésű fájl. Csak .dll fájlok tölthetők fel.",
                        _ => "Ismeretlen hiba."
                    }
                });
        }

        [HttpPost]
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

        private async Task<bool> SaveUserSubmission(ApplicationUser user, IFormFile file)
        {
            FileInfo savedFile = null;
            try
            {
                var timeStamp = DateTime.Now;
                savedFile = await myFileManager.SaveUserSubmissionAsync(user, file, timeStamp);
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

                return true;
            }
            catch (Exception)
            {
                if (savedFile != null)
                {
                    myFileManager.DeleteUserSubmission(user, savedFile.Name);
                }
            }
            return false;
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