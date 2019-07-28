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

            return new JsonResult(new
            {
                submissions = activeSubmissions.Select(x => new
                {
                    id = x.Id,
                    isValid = x.IsValid,
                    uploadDate = x.UploadDate
                }).ToArray()
            });
        }

        private readonly ContestDb myDb;
        private readonly IFileManager myFileManager;
    }
}