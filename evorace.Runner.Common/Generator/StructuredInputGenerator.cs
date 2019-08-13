using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            var text = tree.ToText();

            return new GeneratorResult
            {
                Input = text
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
        string ToText();

        IPart[] Parts { get; }
    }

    public class SimplePart : IPart
    {
        public IPart[] Parts { get; } = new IPart[0];

        public SimplePart(string text)
        {
            myText = text;
        }

        public string ToText() => myText;

        private readonly string myText;
    }

    public class ComplexPart : IPart
    {
        public IPart[] Parts { get; }

        public ComplexPart(params IPart[] parts)
        {
            Parts = parts;
        }

        public string ToText()
        {
            return $"({string.Join(' ', Parts.Select(x => x.ToText()))})";
        }
    }
}
