using System;

namespace evocontest.Runner.Common.Generator
{
    public abstract class InputGeneratorBase
    {
        public InputGeneratorBase(InputGeneratorConfig config)
        {
            myConfig = config;
            myRandom = new Random(myConfig.Seed);
        }

        protected int GetRandomFromRange(MinMaxPair targetPair) => GetRandomFromRange(targetPair, targetPair.Max, 0);

        protected int GetRandomFromRange(MinMaxPair targetPair, int absoluteMax, int minRemaining = 0)
        {
            var maxLength = Math.Min(absoluteMax, targetPair.Max);
            maxLength = maxLength < int.MaxValue ? maxLength + 1 : maxLength;
            var minLength = Math.Min(targetPair.Min, maxLength);
            var length = myRandom.Next(minLength, maxLength);
            length = absoluteMax - length < minRemaining ? absoluteMax : length;

            return length;
        }

        protected void GenerateWord(Span<char> span, bool IsTitleCase = false)
        {
            var length = span.Length;
            if (length == 0) { return; }

            span[0] = IsTitleCase ? GetUpperCaseChar() : GetLowerCaseChar();
            for (var i = 1; i < length; i++)
            {
                span[i] = GetLowerCaseChar();
            }
        }

        protected string GenerateWord(int length, bool isTitleCase = false)
        {
            char[] chars = new char[length];
            GenerateWord(chars.AsSpan(), isTitleCase);
            return new string(chars);
        }

        protected char GetUpperCaseChar() => (char)(myRandom.Next(26) + 65);

        protected char GetLowerCaseChar() => (char)(myRandom.Next(26) + 97);

        protected readonly Random myRandom;
        protected readonly InputGeneratorConfig myConfig;

        protected const char Period = '.';
        protected const char Space = ' ';
    }
}
