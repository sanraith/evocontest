using System.IO;
using System.Threading.Tasks;
using evorace.Runner.Host.Configuration;

namespace evorace.Runner.Host.Core
{
    public sealed class FileManager : IResolvable
    {
        public FileManager(HostConfiguration config)
        {
            myTempDirectory = new DirectoryInfo(config.Directories.Temp);
        }

        public async Task<FileInfo> SaveSubmissionAsync(string submissionId, Stream downloadStream, string fileName)
        {
            var targetDirectory = new DirectoryInfo(Path.Combine(myTempDirectory.FullName, submissionId));
            var fileInfo = new FileInfo(Path.Combine(targetDirectory.FullName, fileName));
            if (!targetDirectory.Exists) { targetDirectory.Create(); }

            await using var fileStream = File.Create(fileInfo.FullName);
            await downloadStream.CopyToAsync(fileStream);

            return fileInfo;
        }
          
        private readonly DirectoryInfo myTempDirectory;
    }
}
