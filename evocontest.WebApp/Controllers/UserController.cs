using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using evocontest.WebApp.Common;
using evocontest.WebApp.Common.Hub;
using evocontest.WebApp.Core;
using evocontest.WebApp.Data;
using evocontest.WebApp.Hubs;
using evocontest.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.SignalR;

namespace evocontest.WebApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        public UserController(UserManager<ApplicationUser> userManager, IFileManager fileManager, ContestDb database,
            IHubContext<WorkerHub, IWorkerHubClient> apiHub)
        {
            myUserManager = userManager;
            myFileManager = fileManager;
            myDb = database;
            myApiHub = apiHub;
        }

        public IActionResult Index() => RedirectToAction(nameof(Submit));

        public IActionResult Submit()
        {
            return View();
        }

        public async Task<IActionResult> CurrentSubmission()
        {
            var user = await myUserManager.GetUserAsync(HttpContext.User);
            var latestSubmission = myDb.Query(user, u => u.Submissions).OrderBy(x => x.UploadDate).LastOrDefault(x => !x.IsDeleted);

            var viewModel = new SubmitViewModel { LatestSubmission = latestSubmission };
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
            var hasActiveSubmission = myDb.Query(user, u => u.Submissions).Any(x => !x.IsDeleted);
            if (hasActiveSubmission)
            {
                // User still has an active submission which has to be deleted first.
                return BadRequest(new { success = false, error = "Már van aktív nevezésed. Frissítsd az oldalt." });
            }

            string submissionId = null;
            var success = false;
            FileManager.SubmissionFileCheckResult checkResult;
            try
            {
                var file = Request.Form.Files.SingleOrDefault();
                checkResult = myFileManager.CheckUserSubmission(file);
                if (checkResult == FileManager.SubmissionFileCheckResult.Ok)
                {
                    submissionId = await SaveUserSubmission(user, file);
                    success = submissionId != null;
                }
            }
            catch (BadHttpRequestException)
            {
                checkResult = FileManager.SubmissionFileCheckResult.InvalidSize;
            }

            if (success)
            {
                await myApiHub.Clients.All.ValidateSubmissions(submissionId);
                return Ok(new { success });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    error = checkResult switch
                    {
                        FileManager.SubmissionFileCheckResult.NoFile => "Hiányzó fájl.",
                        FileManager.SubmissionFileCheckResult.InvalidSize => $"Túl nagy méretű fájl. Maximális méret: {FileManager.MaxSubmittedFileSize / 1024 / 1024} MB.",
                        FileManager.SubmissionFileCheckResult.InvalidFileName => "Helytelen fájlnév. Csak a következő karaktereket használd: A-Z, a-z, 0-9, _.-",
                        FileManager.SubmissionFileCheckResult.InvalidFileNameLength => $"Túl hosszú fájlnév. Maximum {FileManager.MaxFileNameLength} karakter hosszú fájlnevet használj.",
                        FileManager.SubmissionFileCheckResult.InvalidFileExtension => "Hibás kiterjesztésű fájl. Csak .dll fájlok tölthetők fel.",
                        _ => "Ismeretlen hiba."
                    }
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult> DoDelete(string submissionId)
        {
            var user = await myUserManager.GetUserAsync(HttpContext.User);
            var submissionForDelete = myDb.Query(user, u => u.Submissions).OrderBy(x => x.UploadDate).LastOrDefault(x => x.Id == submissionId);

            if (submissionForDelete != null)
            {
                submissionForDelete.IsDeleted = true;
                await myDb.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Submit));
        }

        private async Task<string> SaveUserSubmission(ApplicationUser user, IFormFile file)
        {
            FileInfo savedFile = null;
            try
            {
                var timeStamp = DateTime.Now;
                savedFile = await myFileManager.SaveUserSubmissionAsync(user, file, timeStamp);
                var submission = new Submission
                {
                    User = user,
                    OriginalFileName = FileManager.GetFileName(file),
                    StoredFileName = savedFile.Name,
                    FileSize = (int)savedFile.Length,
                    UploadDate = timeStamp,
                    ValidationState = ValidationStateEnum.Static
                };

                myDb.Submissions.Add(submission);
                myDb.SaveChanges();

                return submission.Id;
            }
            catch (Exception)
            {
                if (savedFile != null)
                {
                    myFileManager.DeleteUserSubmission(user, savedFile.Name);
                }
            }
            return null;
        }

        private readonly ContestDb myDb;
        private readonly IHubContext<WorkerHub, IWorkerHubClient> myApiHub;
        private readonly IFileManager myFileManager;
        private readonly UserManager<ApplicationUser> myUserManager;
    }
}