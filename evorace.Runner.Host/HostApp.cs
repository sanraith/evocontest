using Autofac;
using evorace.Runner.Host.Configuration;
using evorace.Runner.Host.Connection;
using evorace.Runner.Host.Core;
using evorace.Runner.Host.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace evorace.Runner.Host
{
    public sealed class HostApp
    {
        static async Task Main(string[] args)
        {
            if (args.FirstOrDefault() == "--debug")
            {
                Console.WriteLine("Waiting for debugger. Press enter when ready...");
                Console.ReadLine();
            }

            using var container = LoggerExtensions.ProgressLog("Initializing", CreateContainer);
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
            myWebApp.InitSignalR(myHubClient);
            await myWebApp.StartSignalR();

            Console.ReadLine();
        }

        private static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            // Simple resolvables
            static bool IsGenericResolvable(Type i) => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResolvable<>);
            builder.RegisterAssemblyTypes(typeof(HostApp).Assembly)
                .Where(x => typeof(IResolvable).IsAssignableFrom(x) ||
                            x.GetInterfaces().Any(IsGenericResolvable))
                .As(x => typeof(IResolvable).IsAssignableFrom(x)
                        ? x
                        : x.GetInterfaces().First(IsGenericResolvable)
                           .GetGenericArguments().Single());

            // Complex resolvables
            builder.RegisterInstance(HostConfiguration.Load());
            builder.RegisterType<HostApp>().InstancePerLifetimeScope();
            builder.RegisterType<WebAppConnector>().InstancePerLifetimeScope()
                .OnRelease(x => x.DisposeAsync().GetAwaiter().GetResult());

            var container = builder.Build();
            return container;
        }

        private readonly HostConfiguration myConfig;
        private readonly WebAppConnector myWebApp;
        private readonly HubClient myHubClient;
    }
}
