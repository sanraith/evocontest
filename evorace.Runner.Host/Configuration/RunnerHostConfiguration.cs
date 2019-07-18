using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace evorace.Runner.Host.Configuration
{
    public sealed class RunnerHostConfiguration
    {
        public string HostUrl { get; set; } = string.Empty;

        public LoginInformation Login { get; set; } = new LoginInformation();

        public static RunnerHostConfiguration Load()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true, true)
                .Build();

            var configuration = new RunnerHostConfiguration();
            config.Bind(configuration);

            return configuration;
        }
    }
}
