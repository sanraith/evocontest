using System;
using System.IO;
using System.Threading.Tasks;
using evocontest.Runner.Host.Configuration;

namespace evocontest.Runner.Host.Core
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

        public static string GetRelativePath(DirectoryInfo directoryInfo, FileInfo fileInfo)
        {
            var absoluteFolder = directoryInfo.FullName;
            if (!absoluteFolder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                absoluteFolder += Path.DirectorySeparatorChar;
            }

            var relativeUri = new Uri(absoluteFolder).MakeRelativeUri(new Uri(fileInfo.FullName));
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));

            return relativePath;
        }

        public void CleanTempDirectory()
        {
            foreach (var directory in myTempDirectory.GetDirectories())
            {
                try
                {
                    directory.Delete(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not delete \"{directory.FullName}\". {ex.Message}");
                }
            }

            foreach (var file in myTempDirectory.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not delete \"{file.FullName}\". {ex.Message}");
                }
            }
        }

        private readonly DirectoryInfo myTempDirectory;
    }
}
