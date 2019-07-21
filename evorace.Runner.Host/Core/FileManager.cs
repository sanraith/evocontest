using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using evorace.Runner.Host.Configuration;

namespace evorace.Runner.Host.Core
{
    public sealed class FileManager
    {
        public FileManager(HostConfiguration confg)
        {
            myConfg = confg;
            myTempDirectory = new DirectoryInfo(myConfg.Directories.Temp);
        }

        public async Task<FileInfo> SaveSubmission(string submissionId, Stream downloadStream, string fileName)
        {
            var targetDirectory = new DirectoryInfo(Path.Combine(myTempDirectory.FullName, submissionId));
            var fileInfo = new FileInfo(Path.Combine(targetDirectory.FullName, fileName));
            if (!targetDirectory.Exists) { targetDirectory.Create(); }

            using (downloadStream)
            {
                using var fileStream = File.Create(fileInfo.FullName);
                await downloadStream.CopyToAsync(fileStream);
            }

            return fileInfo;
        }
          
        private readonly HostConfiguration myConfg;
        private readonly DirectoryInfo myTempDirectory;
    }
}
