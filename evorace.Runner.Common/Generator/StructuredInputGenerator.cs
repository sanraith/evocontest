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

        public GeneratorResult Generate2(int maxLevel)
        {
            var tree = GenerateRecursivePart();
            var sb = new StringBuilder();
            foreach (IPart part in myParts.OfType<ComplexPart>())
            {
                sb.Append(part.ShortHand).Append('(').Append(part.Render(0)).Append("). ");
            }
            sb.Append(tree.Render(maxLevel));

            Console.WriteLine($"Size: {sb.Length}");

            return new GeneratorResult { Input = sb.ToString() };
        }

        private IPart GenerateRecursivePart()
        {
            var e = new ComplexPart("EEE", new SimplePart("FFFF"));
            var b = new ComplexPart("BB", new SimplePart("DD"), e);
            var a = new ComplexPart("A", b, new SimplePart("CC"));
            e.Parts.Add(a);

            myParts.Add(a);
            myParts.Add(b);
            myParts.Add(e);

            return a;
        }

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

        private HashSet<IPart> myParts = new HashSet<IPart>();
        private List<IPart> myPartList = new List<IPart>();
    }
}
