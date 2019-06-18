using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PerformanceTest.Benchmark.Core
{
    internal sealed class Benchmark : IBenchmark
    {
        public Benchmark(Action action)
        {
            myAction = action;
        }

        public BenchmarkResult Run()
        {
            var runs = new List<TimeSpan>();
            for (int i = 0; i < runCount; i++)
            {
                RunIteration(runs);
            }

            var graph = new AsciiGraph { Height = 10 };
            graph.Items = runs.Select(x => (double)x.Ticks).ToList();
            graph.Draw();

            graph.Items = runs.OrderBy(x => x).Select(x => (double)x.Ticks).ToList();
            graph.Draw();

            graph.Items = runs.OrderBy(x => x).Take(runs.Count - cutCount).Select(x => (double)x.Ticks).ToList();
            graph.Draw();

            runs.Sort();
            var mean = runs
                .Take(runs.Count - cutCount)
                .Average(t => t.Ticks)
                .Transform(avTicks => TimeSpan.FromTicks((long)avTicks));

            return new BenchmarkResult
            {
                Mean = mean,
                Runs = runs
            };
        }


        private void RunIteration(List<TimeSpan> runs)
        {
            myStopWatch.Start();
            myAction.Invoke();
            myStopWatch.Stop();
            runs.Add(myStopWatch.Elapsed);
            myStopWatch.Reset();
        }

        private Action myAction;

        private readonly Stopwatch myStopWatch = new Stopwatch();

        private const int runCount = 50;
        private const int cutCount = (int)(runCount * .2);
    }
}
