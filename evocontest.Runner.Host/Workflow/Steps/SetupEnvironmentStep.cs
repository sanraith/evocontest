using evocontest.Runner.Host.Configuration;
using evocontest.Runner.Host.Core;
using evocontest.Runner.Host.Extensions;
using System.IO;
using System.Linq;

namespace evocontest.Runner.Host.Workflow.Steps
{
    public sealed class SetupEnvironmentStep : IResolvable
    {
        public SetupEnvironmentStep(HostConfiguration config)
        {
            myConfig = config;
        }

        public FileInfo Execute(FileInfo sourceFileInfo)
        {
            var targetFileInfo = LoggerExtensions.WithProgressLog("Setting up environment", () =>
                SetupEnvironment(sourceFileInfo)
            );

            return targetFileInfo;
        }

        private FileInfo SetupEnvironment(FileInfo sourceFile)
        {
            var sourceDirectory = new DirectoryInfo(myConfig.Directories.SubmissionTemplate);
            var targetDirectory = new DirectoryInfo(myConfig.Directories.Submission);

            // clean target
            if (!targetDirectory.Exists) { targetDirectory.Create(); }
            targetDirectory.GetFiles().ToList().ForEach(f => f.Delete());
            targetDirectory.GetDirectories().ToList().ForEach(d => d.Delete(true));

            // copy template directory
            sourceDirectory.GetFiles().ToList().ForEach(f => f.CopyTo(Path.Combine(targetDirectory.FullName, f.Name), true));

            // copy submission dll
            var targetFile = sourceFile.CopyTo(Path.Combine(targetDirectory.FullName, sourceFile.Name), true);

            return targetFile;
        }

        private readonly HostConfiguration myConfig;
    }
}
