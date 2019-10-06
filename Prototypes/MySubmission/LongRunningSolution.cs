using evocontest.Common;
using System;
using System.Threading.Tasks;

namespace MySubmission
{
    public sealed class LongRunningSolution// : ISolution
    {
        public string Solve(string input)
        {
            Task.Delay(TimeSpan.FromDays(1)).Wait();
            return string.Empty;
        }
    }
}
