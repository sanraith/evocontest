using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using evorace.WebApp.Common;
using evorace.WebApp.Common.Data;
using evorace.WebApp.Core;
using evorace.WebApp.Data;
using evorace.WebApp.Data.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace evorace.WebApp.Controllers
{
    [Authorize(Roles = Roles.Worker)]
    public class WorkerController : Controller
    {
        public WorkerController(ContestDb contestDb, IFileManager fileManager)
        {
            myDb = contestDb;
            myFileManager = fileManager;
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

            return new JsonResult(new GetValidSubmissionsResult
            {
                Submissions = activeSubmissions.Select(x => new GetValidSubmissionsResult.Submission
                {
                    Id = x.Id,
                    IsValid = x.IsValid,
                    UploadDate = x.UploadDate
                }).ToList()
            });
        }


        [HttpPost]
        public async Task<IActionResult> UploadMatchResults([Bind(Prefix = "matchResults")] string matchResultsJson)
        {
            var matchResults = JsonSerializer.Deserialize<MatchContainer>(matchResultsJson);
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
    }
}