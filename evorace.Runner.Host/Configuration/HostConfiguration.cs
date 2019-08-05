using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace evorace.Runner.Host.Configuration
{
    public sealed class HostConfiguration
    {
        public string HostUrl { get; set; } = string.Empty;

        public bool UseEpaperDisplay { get; set; } = false;

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
