using evocontest.Common;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MySubmission
{
    public class SimulatedEfficientSolution// : ISolution
    {
        public string Solve(string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }
            var rangePartitioner = Partitioner.Create(0, input.Length);
            var bag = new ConcurrentBag<string>();
            Parallel.ForEach(rangePartitioner, (range, loopstate) =>
            {
                var view = input[range.Item1..range.Item2];
                var part = string.Create(view.Length - view.Replace(" ", "").Length, view, (span, state) =>
                {
                    var pos = 0;
                    var length = state.Length;
                    for (int i = 0; i < length; i++)
                    {
                        if (state[i] == ' ')
                        {
                            span[pos++] = state[i];
                        }
                    }
                });
                bag.Add(part);
            });

            return string.Concat(bag);
        }
    }
}
