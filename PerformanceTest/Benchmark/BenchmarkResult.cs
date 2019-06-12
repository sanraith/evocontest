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
            return $"Mean: {Mean}\nRuns:\n" + string.Join("\n", Runs);
        }
    }
}
