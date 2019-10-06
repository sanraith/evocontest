using System.Diagnostics;

namespace evocontest.Runner.Common.Generator
{
    /// <summary>
    /// Represents a pair of minimum and maximum values.
    /// </summary>
    [DebuggerDisplay("({Min}, {Max})")]
    public sealed class MinMaxPair
    {
        public int Min { get; set; }

        public int Max { get; set; }

        public MinMaxPair(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public MinMaxPair(int minAndMax)
        {
            Min = minAndMax;
            Max = minAndMax;
        }

        private MinMaxPair() { }
    }
}
