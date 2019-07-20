using System.Linq;
using System.Threading.Tasks;
using evorace.WebApp.Core;
using evorace.WebApp.Data;
using evorace.WebApp.Data.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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

        public async Task<IActionResult> DoDownloadSubmission(string submissionId)
        {
            var submission = await myDb.Submissions.Where(x => x.Id == submissionId).Include(x => x.User).FirstOrDefaultAsync();
            if (submission == null) { return NotFound(); }

            var fileInfo = myFileManager.GetFileInfo(submission.User, submission.StoredFileName);
            if (!fileInfo.Exists) { return NotFound(); }

            var fileStream = fileInfo.CreateReadStream();
            return File(fileStream, "application/x-msdownload", submission.OriginalFileName);
        }

        private readonly ContestDb myDb;
        private readonly IFileManager myFileManager;
    }
}