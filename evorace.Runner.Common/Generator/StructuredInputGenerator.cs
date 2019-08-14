using System;
using System.Collections.Generic;
using System.Linq;

namespace evorace.Runner.Common.Generator
{
    public sealed class StructuredInputGenerator : InputGeneratorBase
    {
        public StructuredInputGenerator(InputGeneratorConfig config) : base(config)
        { }

        public GeneratorResult Generate()
        {
            var allParts = new Dictionary<int, IPart>();
            var tree = GeneratePart(30, allParts);
            var span = new char[tree.GetLength()].AsSpan();
            tree.RenderTo(span);

            return new GeneratorResult
            {
                Input = new string(span)
            };
        }

        private IPart GeneratePart(int level, Dictionary<int, IPart> allParts)
        {
            IPart result;
            if (level == 0)
            {
                var wordLength = GetLength(myConfig.WordLength, myConfig.WordLength.Max, 0);
                result = new SimplePart(GenerateWord(wordLength));
            }
            else
            {
                var partCount = myRandom.Next(2, 6); // how many child nodes
                var parts = new IPart[partCount];
                for (int i = 0; i < partCount; i++)
                {
                    IPart part;
                    var shouldReusePart = myRandom.Next(3) == 0 && allParts.Any();
                    if (shouldReusePart)
                    {
                        part = allParts[myRandom.Next(allParts.Count)];
                    }
                    else
                    {
                        var nextLevel = myRandom.Next(Math.Min(0, level - 1), level); // how deep the branch can be
                        part = GeneratePart(nextLevel, allParts);
                    }
                    parts[i] = part;
                }
                result = new ComplexPart(parts);
                allParts.Add(allParts.Count, result);
            }

            return result;
        }
    }

    public interface IPart
    {
        int GetLength();

        int RenderTo(Span<char> span);

        IPart[] Parts { get; }
    }

    public class SimplePart : IPart
    {
        public IPart[] Parts { get; } = new IPart[0];

        public SimplePart(string text)
        {
            myText = text;
        }

        public int RenderTo(Span<char> span)
        {
            myText.AsSpan().CopyTo(span);
            return myText.Length;
        }

        public int GetLength() => myText.Length;

        private readonly string myText;
    }

    public class ComplexPart : IPart
    {
        public IPart[] Parts { get; }

        public ComplexPart(params IPart[] parts)
        {
            Parts = parts;
        }

        public int GetLength()
        {
            const int parentheses = 2;
            var spaces = Parts.Length - 1;

            return parentheses + spaces + Parts.Sum(x => x.GetLength());
        }

        public int RenderTo(Span<char> span)
        {
            var pos = 0;

            span[pos++] = '(';

            var needSpaceBefore = false;
            foreach (var part in Parts)
            {
                if (needSpaceBefore)
                {
                    span[pos++] = ' ';
                }
                else { needSpaceBefore = true; }
                pos += part.RenderTo(span.Slice(pos));
            }

            span[pos++] = ')';

            return pos;
        }
    }
}
