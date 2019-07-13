using evorace.WebApp.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using static evorace.WebApp.Core.FileManager;

namespace evorace.WebApp.Core
{
    public interface IFileManager
    {
        SubmissionFileCheckResult CheckUserSubmission(IFormFile file);

        Task<FileInfo> SaveUserSubmissionAsync(ApplicationUser user, IFormFile file, DateTime timeStamp);

        void DeleteUserSubmission(ApplicationUser user, string fileName);
    }
}
