using System;
using System.Collections.Generic;
using System.Text;

namespace PerformanceTest.Benchmark
{
    public sealed class BenchmarkResult
    {
        public TimeSpan Mean { get; set; }
        public IReadOnlyList<TimeSpan> Runs { get; set; }

        public override string ToString()
        {
            //return $"Runs:\n" + string.Join("\n", Runs) + $"\nMean: {Mean}";
            return $"\nMean: {Mean}";
        }
    }
}
