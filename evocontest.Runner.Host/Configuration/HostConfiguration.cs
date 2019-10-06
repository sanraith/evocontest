using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace evocontest.Runner.Host.Configuration
{
    public sealed class HostConfiguration
    {
        public string HostUrl { get; set; } = string.Empty;

        public bool UseEpaperDisplay { get; set; } = false;

        public bool UseFanControl { get; set; } = false;

        public bool IsDebug { get; set; } = false;
        
        public int FanGpio { get; set; } = 26;

        public int CoolDownSeconds { get; set; } = 30;

        public int SingleSolveTimeoutMillis { get; set; } = 5000;

        public int MaxRoundSolutionTimeMillis { get; set; } = 5000;

        public int WarmupTimeoutMillis { get; set; } = 1000;

        public Directories Directories { get; set; } = new Directories();

        public CustomProcessStartInfo WorkerProcessInfo { get; set; } = new CustomProcessStartInfo();

        public LoginInformation Login { get; set; } = new LoginInformation();

        public static HostConfiguration Load()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true, true)
                .Build();

            var configuration = new HostConfiguration();
            config.Bind(configuration);

            return configuration;
        }
    }
}
