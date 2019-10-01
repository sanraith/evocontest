namespace evocontest.Runner.Common.Generator
{
    public sealed class MinMaxPair
    {
        public int Min { get; }

        public int Max { get; }

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
    }
}
