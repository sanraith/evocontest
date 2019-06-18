using System;

namespace PerformanceTest
{
    internal sealed class PerformanceTestProgram
    {
        static void Main(string[] args)
        {
            var benchmark = new Benchmark.Core.Benchmark(Spin);
            var result = benchmark.Run();
            Console.WriteLine(result);
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
