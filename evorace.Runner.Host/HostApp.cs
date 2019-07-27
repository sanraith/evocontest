using Autofac;
using evorace.Runner.Host.Configuration;
using evorace.Runner.Host.Connection;
using evorace.Runner.Host.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace evorace.Runner.Host
{
    public sealed class HostApp : IResolvable
    {
        static async Task Main(string[] args)
        {
            if (args.FirstOrDefault() == "--debug")
            {
                Console.WriteLine("Press enter when ready...");
                Console.ReadLine();
            }

            var container = await Task.Run(() => CreateContainer()).WithProgressLog("Initializing");
            using (var scope = container.BeginLifetimeScope())
            {
                var app = scope.Resolve<HostApp>();
                await app.Run();
            }
        }

        public HostApp(HostConfiguration config, WebAppConnector webApp, HubClient hubClient)
        {
            myConfig = config;
            myWebApp = webApp;
            myHubClient = hubClient;
        }

        private async Task Run()
        {
            await myWebApp.Login(myConfig.Login.Email, myConfig.Login.Password);
            var hubProxy = myWebApp.InitSignalR(myHubClient);
            myHubClient.WorkerHubServer = hubProxy;
            await myWebApp.StartSignalR();

            Console.ReadLine();
            await myWebApp.DisposeAsync();
        }

        private static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(HostConfiguration.Load());
            builder.RegisterAssemblyTypes(typeof(HostApp).Assembly)
                .Where(x => typeof(IResolvable).IsAssignableFrom(x))
                .AsSelf();
            builder.RegisterAssemblyTypes(typeof(HostApp).Assembly)
                .Where(x => x.GetInterfaces().Any(i =>
                                i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IResolvable<>)))
                .As(x => x.GetInterfaces().First(i =>
                               i.IsGenericType &&
                               i.GetGenericTypeDefinition() == typeof(IResolvable<>))
                          .GetGenericArguments().First());

            var container = builder.Build();

            return container;
        }

        private readonly HostConfiguration myConfig;
        private readonly WebAppConnector myWebApp;
        private readonly HubClient myHubClient;
    }
}
