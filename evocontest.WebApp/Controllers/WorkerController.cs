using System;
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
        public WorkerController(ContestDb contestDb, ISubmissionManager submissionManager)
        {
            myDb = contestDb;
            mySubmissionManager = submissionManager;
        }

        public async Task<IActionResult> DownloadSubmission(string submissionId)
        {
            var (fileStream, originalFileName) = await mySubmissionManager.DownloadSubmission(submissionId);
            if (fileStream == null)
            {
                return NotFound();
            }
            return File(fileStream, "application/x-msdownload", originalFileName);
        }

        public async Task<JsonResult> GetValidSubmissions()
        {
            var getValidSubmissionResult = await mySubmissionManager.GetValidSubmissions();
            return new JsonResult(getValidSubmissionResult);
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
        private readonly ISubmissionManager mySubmissionManager;
    }
}