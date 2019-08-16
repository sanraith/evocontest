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
            var tree = GeneratePart(15);

            var sb = new StringBuilder();
            foreach (IPart part in myParts.OfType<ComplexPart>())
            {
                sb.Append(part.ShortHand).Append('(').Append(part.Render()).Append("). ");
            }
            sb.Append(tree.Render());

            return new GeneratorResult
            {
                Input = sb.ToString()
            };
        }

        private HashSet<IPart> myParts = new HashSet<IPart>();
        private List<IPart> myPartList = new List<IPart>();

        private IPart GeneratePart(int level)
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
                    var shouldReusePart = myRandom.Next(3) == 0 && myParts.Count > 0;
                    if (shouldReusePart)
                    {
                        part = myPartList[myRandom.Next(myPartList.Count)];
                    }
                    else
                    {
                        var nextLevel = myRandom.Next(Math.Min(0, level - 1), level); // how deep the branch can be
                        part = GeneratePart(nextLevel);
                    }
                    parts[i] = part;
                    if (myParts.Add(part))
                    {
                        myPartList.Add(part);
                    }
                }

                string shortHand;
                do
                {
                    var wordLength = GetLength(myConfig.WordLength, myConfig.WordLength.Max, 0);
                    shortHand = GenerateWord(wordLength);
                } while (myParts.Any(x => x.ShortHand == shortHand));
                result = new ComplexPart(shortHand, parts);
            }

            return result;
        }
    }

    public interface IPart
    {
        int GetLength();

        int RenderTo(Span<char> span);

        IPart[] Parts { get; }

        string ShortHand { get; }

        string Render()
        {
            var partSpan = new char[GetLength()].AsSpan();
            RenderTo(partSpan);
            return new string(partSpan);
        }
    }

    public class SimplePart : IPart
    {
        public string ShortHand { get; }

        public IPart[] Parts { get; } = new IPart[0];

        public SimplePart(string text)
        {
            ShortHand = text;
        }

        public int RenderTo(Span<char> span)
        {
            ShortHand.AsSpan().CopyTo(span);
            return ShortHand.Length;
        }

        public int GetLength() => ShortHand.Length;
    }

    public class ComplexPart : IPart
    {
        public string ShortHand { get; }

        public IPart[] Parts { get; }

        public ComplexPart(string shortHand, params IPart[] parts)
        {
            ShortHand = shortHand;
            Parts = parts;
        }

        public int GetLength()
        {
            var spaces = Parts.Length - 1;

            return spaces + ShortHand.Length + Parts.Sum(x => x.GetLength());
        }

        public int RenderTo(Span<char> span)
        {
            var pos = 0;

            ShortHand.AsSpan().CopyTo(span);
            pos += ShortHand.Length;

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

            return pos;
        }
    }
}
