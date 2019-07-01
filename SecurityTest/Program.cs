using System;
using System.IO;
using System.Linq;
using System.Net;

namespace SecurityTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Any())
            {
                Console.WriteLine("Sandbox");
                new Sandboxed().Run();
            }
            else
            {
                Console.WriteLine("Launcher");
                new Launcher().Run();
            }
        }
    }


    public sealed class Launcher
    {
        public void Run()
        { }
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
