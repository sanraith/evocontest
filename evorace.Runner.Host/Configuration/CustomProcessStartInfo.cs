using System.Diagnostics;

namespace evorace.Runner.Host.Configuration
{
    public sealed class CustomProcessStartInfo
    {
        public string FileName { get; set; } = string.Empty;

        public string Arguments { get; set; } = string.Empty;

        public string? WorkingDirectory { get; set; }

        public static implicit operator ProcessStartInfo(CustomProcessStartInfo x)
        {
            return new ProcessStartInfo
            {
                FileName = x.FileName,
                Arguments = x.Arguments,
                WorkingDirectory = x.WorkingDirectory
            };
        }
    }
}
