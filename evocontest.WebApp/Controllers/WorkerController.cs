﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using evocontest.WebApp.Common;
using evocontest.WebApp.Common.Data;
using evocontest.WebApp.Core;
using evocontest.WebApp.Data;
using evocontest.WebApp.Data.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace evocontest.WebApp.Controllers
{
    [Authorize(Roles = Roles.Worker)]
    public class WorkerController : Controller
    {
        public WorkerController(ContestDb contestDb, IFileManager fileManager, UserManager<ApplicationUser> userManager)
        {
            myDb = contestDb;
            myFileManager = fileManager;
            myUserManager = userManager;
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

        public async Task<JsonResult> GetValidSubmissions()
        {
            IEnumerable<Submission> activeSubmissions = await myDb.Submissions
                .Where(x => !x.IsDeleted && (x.IsValid ?? false))
                .Include(x => x.User)
                .ToListAsync();

            // Be safe, but there should be only one active submission per user.
            activeSubmissions = activeSubmissions
                .GroupBy(x => x.User)
                .Select(x => x.OrderBy(x => x.UploadDate).Last());

            var adminUsers = await myUserManager.GetUsersInRoleAsync(Roles.Admin);

            return new JsonResult(new GetValidSubmissionsResult
            {
                Submissions = activeSubmissions.Select(x => new GetValidSubmissionsResult.Submission
                {
                    Id = x.Id,
                    FullName = x.User.FullName,
                    IsAdmin = adminUsers.Contains(x.User),
                    IsValid = x.IsValid,
                    UploadDate = x.UploadDate
                }).ToList()
            });
        }


        [HttpPost]
        public async Task<IActionResult> UploadMatchResults([FromBody] MatchContainer matchResults)
        {
            var requiredSubmissionIds = matchResults.Measurements.Select(x => x.SubmissionId).ToList();
            var submissions = await myDb.Submissions
                .Where(x => requiredSubmissionIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x);

            var match = new Match
            {
                MatchDate = DateTime.Now,
                JsonResult = JsonSerializer.Serialize(matchResults)
            };
            await myDb.Matches.AddAsync(match);

            foreach (var mContainer in matchResults.Measurements)
            {
                var measurement = new Measurement
                {
                    Match = match,
                    Submission = submissions[mContainer.SubmissionId],
                    JsonResult = JsonSerializer.Serialize(mContainer)
                };
                await myDb.Measurements.AddAsync(measurement);
            }

            await myDb.SaveChangesAsync();
            return Ok();
        }

        private readonly ContestDb myDb;
        private readonly IFileManager myFileManager;
        private readonly UserManager<ApplicationUser> myUserManager;
    }
}