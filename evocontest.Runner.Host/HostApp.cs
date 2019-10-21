using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using evocontest.Runner.Host.Configuration;
using evocontest.Runner.Host.Connection;
using evocontest.Runner.Host.Core;
using evocontest.Runner.Host.Extensions;
using evocontest.Runner.Host.Workflow;
using evocontest.Runner.RaspberryPiUtilities;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using ImageSharpConfig = SixLabors.ImageSharp.Configuration;

namespace evocontest.Runner.Host
{
    public sealed class HostApp
    {
        static async Task Main(string[] args)
        {
            var isDebug = string.Equals(args.FirstOrDefault(), "--debug", StringComparison.OrdinalIgnoreCase);
            if (isDebug)
            {
                Console.WriteLine("Waiting for debugger. Press enter when ready...");
                Console.ReadLine();
                args = args[1..];
            }
            var isMatch = string.Equals(args.FirstOrDefault(), "--match", StringComparison.OrdinalIgnoreCase);

            using var container = LoggerExtensions.ProgressLog("Initializing", CreateContainer);
            using (var scope = container.BeginLifetimeScope())
            {
                var config = container.Resolve<HostConfiguration>();
                config.IsDebug = config.IsDebug ? true : isDebug;

                var screen = container.Resolve<IEpaperDisplay>();
                using (var image = new Image<Rgba32>(ImageSharpConfig.Default, screen.Width, screen.Height, Rgba32.White))
                {
                    image.Mutate(x => x.DrawText(new TextGraphicsOptions { Antialias = false },
                        "evocontest Runner",
                        RpiFonts.Roboto.CreateFont(24), Rgba32.Black, new PointF(10, 10)));

                    await screen.InitializeAsync(RefreshMode.Full);
                    await screen.DrawAsync(image.ToBlackAndWhitePixels());
                    await screen.SleepAsync();
                }

                if (isMatch)
                {
                    var matchFilePath = args.Length > 1 ? args[1] : "match.zip";
                    await LiveMatchWorkflow(matchFilePath, scope);
                }
                else
                {
                    await MainWorkflow(scope);
                }
            }
        }

        private static async Task LiveMatchWorkflow(string matchFilePath, ILifetimeScope scope)
        {
            var workflow = scope.Resolve<LiveMatchWorkflow>();
            await workflow.RunAsync(matchFilePath);
        }

        private static async Task MainWorkflow(ILifetimeScope scope)
        {
            var workflow = scope.Resolve<MainWorkflow>();
            await workflow.ExecuteAsync();
        }

        private static IContainer CreateContainer()
        {
            var config = HostConfiguration.Load();
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
            builder.RegisterInstance(config);
            builder.RegisterType<HostApp>().InstancePerLifetimeScope();
            builder.RegisterType<WebAppConnector>().InstancePerLifetimeScope()
                .OnRelease(x => x.DisposeAsync().GetAwaiter().GetResult());

            // Conditional resolvables
            if (config.UseEpaperDisplay)
            {
                builder.RegisterType<Waveshare213EpaperDisplay>().As<IEpaperDisplay>().SingleInstance()
                    .OnRelease(x =>
                    {
                        x.ClearAsync().GetAwaiter().GetResult();
                        x.DisposeAsync().GetAwaiter().GetResult();
                    });
            }
            else
            {
                builder.RegisterType<DummyEpaperDisplay>().As<IEpaperDisplay>();
            }
            if (config.UseFanControl)
            {
                builder.RegisterInstance(new FanHandler(config.FanGpio)).As<IFanControl>().SingleInstance();
            }
            else
            {
                builder.RegisterType<DummyFanControl>().As<IFanControl>();
            }


            var container = builder.Build();
            return container;
        }
    }
}
