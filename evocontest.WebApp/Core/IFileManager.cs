using evocontest.WebApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Threading.Tasks;
using static evocontest.WebApp.Core.FileManager;

namespace evocontest.WebApp.Core
{
    public interface IFileManager
    {
        SubmissionFileCheckResult CheckUserSubmission(IFormFile file);

        Task<FileInfo> SaveUserSubmissionAsync(ApplicationUser user, IFormFile file, DateTime timeStamp);

        Task<IFileInfo> SaveFileAsync(ApplicationUser user, string fileName, Stream stream);

        void DeleteUserSubmission(ApplicationUser user, string fileName);

        IFileInfo GetFileInfo(ApplicationUser user, string fileName);
    }
}
