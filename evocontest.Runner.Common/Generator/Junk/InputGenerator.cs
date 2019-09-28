using System;

namespace evocontest.Runner.Common.Generator
{
    public sealed class InputGenerator : InputGeneratorBase
    {
        public InputGenerator(InputGeneratorConfig config) : base(config)
        { }

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
                var sentenceLength = GetRandomFromRange(sentenceTargetLength, absoluteMax: length - pos, minRemaining: sentenceTargetLength.Min + 1);
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
                var wordLength = GetRandomFromRange(wordTargetLength, absoluteMax: length - pos, minRemaining: wordTargetLength.Min + 1);
                GenerateWord(span.Slice(pos, wordLength), pos == 0);
                pos += wordLength + 1;
                if (pos - 1 < length) { span[pos - 1] = Space; }
            }
            span[^1] = Period;
        }
    }
}
