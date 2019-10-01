using evocontest.Common;
using System.Collections.Generic;
using System.Linq;

namespace MySubmission
{
    public class SimulatedEfficientSolution : ISolution
    {
        public string Solve(string input)
        {
            var wrongButFastSolution = string.Create(input.Length - input.Replace(" ", "").Length, input, (span, state) =>
            {
                var pos = 0;
                var length = state.Length;
                for (int i = 0; i < length; i++)
                {
                    if (state[i] == ' ')
                    {
                        span[pos++] = state[i + 1];
                    }
                }
            });

            return wrongButFastSolution;
        }
    }
}
