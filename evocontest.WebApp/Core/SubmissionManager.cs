using evocontest.WebApp.Common;
using evocontest.WebApp.Data;
using evocontest.WebApp.Data.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace evocontest.WebApp.Core
{
    public sealed class SubmissionManager : ISubmissionManager
    {
        public SubmissionManager(IFileManager fileManager, ContestDb db, UserManager<ApplicationUser> userManager)
        {
            myDb = db;
            myFileManager = fileManager;
            myUserManager = userManager;
        }

        public async Task<(Stream?, string?)> DownloadSubmission(string submissionId)
        {
            var submission = await myDb.Submissions.Where(x => x.Id == submissionId).Include(x => x.User).FirstOrDefaultAsync();
            if (submission == null) { return (null, null); }

            var fileInfo = myFileManager.GetFileInfo(submission.User, submission.StoredFileName);
            if (!fileInfo.Exists) { return (null, null); }

            var fileStream = fileInfo.CreateReadStream();
            return (fileStream, submission.OriginalFileName);
        }

        public async Task<GetValidSubmissionsResult> GetValidSubmissions()
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

            return new GetValidSubmissionsResult
            {
                Submissions = activeSubmissions.Select(x => new GetValidSubmissionsResult.Submission
                {
                    Id = x.Id,
                    UserName = x.User.FullName,
                    IsAdmin = adminUsers.Contains(x.User),
                    FileName = $"{x.Id}.dll",
                    OriginalFileName = x.OriginalFileName,
                    UploadDate = x.UploadDate
                }).ToList()
            };
        }

        private readonly IFileManager myFileManager;
        private readonly ContestDb myDb;
        private readonly UserManager<ApplicationUser> myUserManager;
    }
}
