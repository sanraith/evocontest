using System;

namespace evorace.Runner.Common.Generator
{
    public sealed class InputGenerator
    {
        public InputGenerator(InputGeneratorConfig config)
        {
            myConfig = config;
            myRandom = new Random(myConfig.Seed);
        }

        public GeneratorResult Generate()
        {
            var inputSpan = new char[myConfig.InputLength].AsSpan();
            GenerateSentences(inputSpan);
            var inputString = new string(inputSpan);

            return new GeneratorResult
            {
                Input = inputString,
                Output = inputString
            };
        }

        private void GenerateSentences(Span<char> span)
        {
            var length = span.Length;

            var sentenceTargetLength = myConfig.SentenceLength;
            var pos = 0;
            while (pos < length)
            {
                var sentenceLength = GetLength(sentenceTargetLength, absoluteMax: length - pos, minRemaining: sentenceTargetLength.Min + 1);
                GenerateSentence(span.Slice(pos, sentenceLength));
                pos += sentenceLength + 1;
                if (pos - 1 < length) { span[pos - 1] = Space; }
            }
        }

        private void GenerateSentence(Span<char> span)
        {
            var length = span.Length - 1;
            if (length <= 0) { return; }

            var wordTargetLength = myConfig.WordLength;
            int pos = 0;
            while (pos < length)
            {
                var wordLength = GetLength(wordTargetLength, absoluteMax: length - pos, minRemaining: wordTargetLength.Min + 1);
                GenerateWord(span.Slice(pos, wordLength), pos == 0);
                pos += wordLength + 1;
                if (pos - 1 < length) { span[pos - 1] = Space; }
            }
            span[^1] = Period;
        }

        private int GetLength(MinMaxPair targetLength, int absoluteMax, int minRemaining)
        {
            var maxLength = Math.Min(absoluteMax, targetLength.Max);
            var minLength = Math.Min(targetLength.Min, maxLength);
            var length = myRandom.Next(minLength, maxLength);
            length = absoluteMax - length < minRemaining ? absoluteMax : length;

            return length;
        }

        public void GenerateWord(Span<char> span, bool IsTitleCase = false)
        {
            var length = span.Length;
            if (length == 0) { return; }

            span[0] = IsTitleCase ? GetUpperCaseChar() : GetLowerCaseChar();
            for (var i = 1; i < length; i++)
            {
                span[i] = GetLowerCaseChar();
            }
        }

        private char GetUpperCaseChar() => (char)(myRandom.Next(26) + 65);

        private char GetLowerCaseChar() => (char)(myRandom.Next(26) + 97);

        private readonly Random myRandom;
        private readonly InputGeneratorConfig myConfig;

        private const char Period = '.';
        private const char Space = ' ';
    }
}
