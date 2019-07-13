using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LoginTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .Build();

            using var connector = new WebAppConnector();
            await connector.Login(config["Login.Email"], config["Login.Password"]);
        }
    }
}
