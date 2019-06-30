using System;
using System.IO;
using System.Linq;

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

    public sealed class Sandboxed
    {
        public void Run()
        {
            try
            {
                using (var s = File.CreateText("test.txt"))
                {
                    s.WriteLine("Hello");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Could not create new file");
            }
            try
            {
                File.WriteAllText("output.txt", "hello");
            }
            catch (Exception)
            {
                Console.WriteLine("Could not write to output");
            }

        }
    }

    public sealed class Launcher
    {
        public void Run()
        { }
    }
}
