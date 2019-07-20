using evorace.WebApp.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace evorace.WebApp.Core
{
    public class FileManager : IFileManager
    {
        public const int MaxSubmittedFileSize = 5 * 1024 * 1024;
        public const int MaxFileNameLength = 100;

        public FileManager(IWebHostEnvironment environment)
        {
            myRootPath = environment.ContentRootPath;
            myStoreRootPath = Path.Combine(myRootPath, StorageFolderName);
            myEnvironment = environment;
        }

        public SubmissionFileCheckResult CheckUserSubmission(IFormFile file)
        {
            if (file == null)
            {
                return SubmissionFileCheckResult.NoFile;
            }

            if (file.Length <= 0 || file.Length >= MaxSubmittedFileSize)
            {
                return SubmissionFileCheckResult.InvalidSize;
            }

            if (file.FileName.Length <= 0 || file.FileName.Length > MaxFileNameLength)
            {
                return SubmissionFileCheckResult.InvalidFileNameLength;
            }

            if (!string.Equals(".dll", new FileInfo(file.FileName).Extension, StringComparison.OrdinalIgnoreCase))
            {
                return SubmissionFileCheckResult.InvalidFileExtension;
            }

            var fileNameRegex = new Regex(@"^[\w\-. ]+\.(?i)(dll)$");
            if (!fileNameRegex.IsMatch(file.FileName))
            {
                return SubmissionFileCheckResult.InvalidFileName;
            }

            return SubmissionFileCheckResult.Ok;
        }

        public async Task<FileInfo> SaveUserSubmissionAsync(ApplicationUser user, IFormFile file, DateTime timeStamp)
        {
            var userDir = GetUserDir(user);
            if (!userDir.Exists)
            {
                userDir.Create();
            }

            var fileName = CreateFileName(timeStamp);
            var fileInfo = new FileInfo(Path.Combine(userDir.FullName, fileName));

            using (var readStream = file.OpenReadStream())
            using (var writeStream = File.Create(fileInfo.FullName))
            {
                await readStream.CopyToAsync(writeStream);
            }

            return fileInfo;
        }

        public void DeleteUserSubmission(ApplicationUser user, string fileName)
        {
            var userDir = GetUserDir(user);
            var file = new FileInfo(Path.Combine(userDir.FullName, fileName));

            if (file.Exists)
            {
                file.Delete();
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        public IFileInfo GetFileInfo(ApplicationUser user, string fileName)
        {
            return myEnvironment.ContentRootFileProvider.GetFileInfo(Path.Combine(StorageFolderName, SubmissionFolderName, user.UploadFolderName, fileName));
        }

        private DirectoryInfo GetUserDir(ApplicationUser user)
        {
            return new DirectoryInfo(Path.Combine(myStoreRootPath, SubmissionFolderName, user.UploadFolderName));
        }

        private static string CreateFileName(DateTime timeStamp)
        {
            return $"sub_{timeStamp.ToString("yyyyMMdd_HHmmss_fffffff")}.dll";
        }

        public enum SubmissionFileCheckResult
        {
            Ok,
            NoFile,
            InvalidSize,
            InvalidFileName,
            InvalidFileExtension,
            InvalidFileNameLength
        }

        private readonly string myRootPath;
        private readonly string myStoreRootPath;
        private readonly IWebHostEnvironment myEnvironment;
        private const string StorageFolderName = "_Store";
        private const string SubmissionFolderName = "Submissions";
    }
}
