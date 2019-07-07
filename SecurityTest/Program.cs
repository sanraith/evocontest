using evowar.Runner.Common.Commands;
using evowar.Runner.Common.Connection;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SecurityTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("Launcher");
                new Launcher().Run();
            }
            else if (args[0] == "wait")
            {
                new Waiter().Run();
            }
            else if (args[0] == "pipe")
            {
                new Piper().Run();
            }
            else
            {
                Console.WriteLine("Sandbox");
                new Sandboxed().Run();
            }
        }
    }


    public sealed class Piper
    {
        public void Run()
        {
            var pipeName = "pipe";

            _ = Task.Run(async () =>
            {
                using var pipeServer = new PipeServer(pipeName);
                await pipeServer.WaitForConnectionAsync();
                Console.WriteLine("Start sending...");
                await pipeServer.SendCommandAsync(new LoadContextCommand { ContesterAssemblyName = "asd.dll" });
                Console.WriteLine("Done sending");
            });

            Task.Run(async () =>
            {
                using var pipeClient = new PipeClient(pipeName);
                await pipeClient.ConnectAsync();
                Console.WriteLine("Start receiving...");
                var command = await pipeClient.ReceiveCommandAsync();
                Console.WriteLine("Done receiving");

                switch (command)
                {
                    case LoadContextCommand loadCommand:
                        Console.WriteLine(loadCommand.ContesterAssemblyName);
                        break;
                }
            }).Wait();
        }
    }

    public sealed class Launcher
    {
        public void Run()
        {
            var p = new Process();
            p.StartInfo.FileName = "dotnet";
            p.StartInfo.Arguments = "SecurityTest.dll wait";
            p.Start();
        }
    }

    public sealed class Waiter
    {
        public void Run()
        {
            Console.WriteLine("Waiting a lot...");
            Task.Delay(TimeSpan.FromMinutes(5)).Wait();
        }
    }

    public sealed class Sandboxed
    {
        public void Run()
        {
            var isSandbox = true;
            isSandbox &= AssertDenied("read from outside of sandbox", () =>
            {
                var secrets = File.ReadAllText("~/secrets.txt");
            });
            isSandbox &= AssertDenied("create new file", () =>
            {
                using (var s = File.CreateText("test.txt"))
                {
                    s.WriteLine("Hello");
                }
            });
            isSandbox &= AssertDenied("access to the internet", () =>
            {
                WebClient client = new WebClient();
                string downloadString = client.DownloadString("http://www.gooogle.com");
            });

            Console.WriteLine($"Is sandbox: {isSandbox}");
        }

        public bool AssertDenied(string desc, Action a)
        {
            try
            {
                a();
                Console.WriteLine($"ERROR - no exception for {desc}");
                return false;
            }
            catch (Exception)
            {
                Console.WriteLine($"OK - {desc} not allowed.");
                return true;
            }
        }
    }
}
