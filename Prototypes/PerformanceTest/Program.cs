using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MySubmission;

namespace PerformanceTest
{
    internal sealed class PerformanceTestProgram
    {
        private const int Max = 500000000;
        private const int Max2 = 20000000;

        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            var str = new string('a', Max2).ToArray();
            var strs = Enumerable.Range(0, 8).Select(_ => new string(str).ToArray()).ToArray();
            var repeatCount = 50;

            sw.Restart();
            Console.WriteLine("single");
            var (from, to) = (0, Max2);
            for (int j = 1; j < repeatCount; j++)
            {
                var sum = 0;
                for (var i = from; i < to; i++)
                {
                    sum += str[i] == 'a' ? 1 : 0;
                }
            }
            Console.WriteLine($"Done in {sw.ElapsedMilliseconds}");

            Console.WriteLine("copy");
            sw.Restart();
            for (int i = 1; i < repeatCount; i++)
            {
                CopyOrNot(str, strs, true);
            }
            Console.WriteLine($"Done in {sw.ElapsedMilliseconds}");

            Console.WriteLine("no copy");
            sw.Restart();
            for (int i = 1; i < repeatCount; i++)
            {
                CopyOrNot(str, strs, false);
            }
            Console.WriteLine($"Done in {sw.ElapsedMilliseconds}");

            //Console.WriteLine("Single core...");
            //var sw = Stopwatch.StartNew();
            //SingleCore();
            //sw.Stop();
            //Console.WriteLine($"Done in {sw.ElapsedMilliseconds}");


            //Console.WriteLine("Multi core 2...");
            //sw.Restart();
            //MultiThreaded(2);
            //sw.Stop();
            //Console.WriteLine($"Done in {sw.ElapsedMilliseconds}");

            //Console.WriteLine("Multi core 4...");
            //sw.Restart();
            //MultiThreaded(4);
            //sw.Stop();
            //Console.WriteLine($"Done in {sw.ElapsedMilliseconds}");

            //Console.WriteLine("Multi core 8...");
            //sw.Restart();
            //MultiThreaded(8);
            //sw.Stop();
            //Console.WriteLine($"Done in {sw.ElapsedMilliseconds}");


            //Console.WriteLine("Multi core 16...");
            //sw.Restart();
            //MultiThreaded(16);
            //sw.Stop();
            //Console.WriteLine($"Done in {sw.ElapsedMilliseconds}");



            //var input = "Alma Aroma Bor Bagatell. Alma Aroma Apple. AA Bor.";
            //Console.WriteLine(input);
            //var solved = new MyNewSolution().Solve(input);
            //Console.WriteLine(solved);

            //var input = "A PDSC(Patient Data Service Client) segítségével lehet csatlakozni a Patient Data Servicehez. A világ legjobb dolga a PDS(Patient Data Service). Tehát a PDS Client az egyik leghasznosabb dolog.";
            //Console.WriteLine(input);
            //var solved = new MySolution().Solve(input);

            //var benchmark = new Benchmark.Core.Benchmark(Spin);
            //var result = benchmark.Run();
            //Console.WriteLine(result);
        }

        private static void CopyOrNot(char[] origStr, char[][] strs, bool isCopy)
        {
            var chunks = 4;
            var ranges = Partitioner.Create(0, Max2, (int)Math.Ceiling(Max2 / (double)chunks));
            //var bag = new ConcurrentBag<int>();

            Parallel.ForEach(ranges, (range, loopState, loopIndex) =>
            {
                var str = isCopy ? strs[loopIndex * 2] : origStr;
                var (from, to) = range;
                var sum = 0;
                for (var i = from; i < to; i++)
                {
                    sum += str[i] == 'a' ? 1 : 0;
                }
                //bag.Add(sum);
            });
        }

        private static void MultiThreaded(int chunks)
        {
            var ranges = Partitioner.Create(0, Max, Max / chunks);
            var bag = new ConcurrentBag<int>();
            Parallel.ForEach(ranges, (range, loopState) =>
            {
                var (from, to) = range;
                var sum = 0;
                for (var i = from; i < to; i++)
                {
                    sum += (int)Math.Pow(-1, 1 + i % 2);
                }
                bag.Add(sum);
            });
            var total = bag.Sum();
        }

        private static void SingleCore()
        {
            var sum = 0;
            for (int i = 0; i < Max; i++)
            {
                sum += (int)Math.Pow(-1, 1 + i % 2);
            }
        }

        private static void Spin()
        {
            const double c = 987.654;
            double number = 12345.6789;
            for (int i = 0; i < 1000000; i++)
            {
                number *= c;
                number /= c;
            }
        }
    }
}
