using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MySubmission;

namespace PerformanceTest
{
    internal sealed class PerformanceTestProgram
    {
        static void Main(string[] args)
        {
            var input = "Alma Aroma Bor Bagatell. Alma Aroma Apple. AA Bor.";
            Console.WriteLine(input);
            var solved = new MyNewSolution().Solve(input);
            //Console.WriteLine(solved);

            //var input = "A PDSC(Patient Data Service Client) segítségével lehet csatlakozni a Patient Data Servicehez. A világ legjobb dolga a PDS(Patient Data Service). Tehát a PDS Client az egyik leghasznosabb dolog.";
            //Console.WriteLine(input);
            //var solved = new MySolution().Solve(input);

            //var benchmark = new Benchmark.Core.Benchmark(Spin);
            //var result = benchmark.Run();
            //Console.WriteLine(result);
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
