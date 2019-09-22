using evocontest.Runner.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace evocontest.Runner.Common.Generator
{
    public sealed class StructuredInputGenerator : InputGeneratorBase
    {
        public StructuredInputGenerator(InputGeneratorConfig config) : base(config)
        { }

        public GeneratorResult Generate(int maxLevel)
        {
            var sw = Stopwatch.StartNew();

            var tree = GenerateComplexRecursiveParts();
            Console.WriteLine($"Generate: {sw.ElapsedMilliseconds}"); sw.Restart();

            var sb = new StringBuilder();
            foreach (IPart part in myParts.OfType<ComplexPart>())
            {
                sb.Append(part.ShortHand).Append('(').Append(part.Render(0)).Append("). ");
            }
            Console.WriteLine($"Header: {sw.ElapsedMilliseconds}"); sw.Restart();

            var partSpan = new char[myConfig.InputLength].AsSpan();
            var length = tree.RenderTo(partSpan, maxLevel);
            partSpan = partSpan.Slice(0, length);
            Console.WriteLine($"Render: {sw.ElapsedMilliseconds}"); sw.Restart();
            Console.WriteLine($"SpanLength: {partSpan.Length}"); sw.Restart();

            sb.Append(partSpan);
            Console.WriteLine($"Length: {sb.Length}");

            return new GeneratorResult { Input = sb.ToString() };
        }

        //private IPart GetHardCodedRecursiveParts()
        //{
        //    var e = new ComplexPart("EEE", new ComplexPart("FFFF"));
        //    var b = new ComplexPart("BB", new ComplexPart("DD"), e);
        //    var a = new ComplexPart("A", b, new ComplexPart("CC"));
        //    e.Parts.Add(a);

        //    myParts.Add(a);
        //    myParts.Add(b);
        //    myParts.Add(e);

        //    return a;
        //}

        private IPart GenerateComplexRecursiveParts()
        {
            var levelCount = 5;
            var recursivityPercent = .2;
            var recursiveJumpDistance = new MinMaxPair(2, 5);
            var levelDiversity = new MinMaxPair(5, 10);
            var childRange = new MinMaxPair(2, 4);

            var levels = CreateLevels(levelCount, levelDiversity, childRange);

            RemoveUnusedParts(levels);

            AddRecursivity(levels, recursivityPercent, recursiveJumpDistance);

            // TODO add checks for unwanted references

            levels.SelectMany(x => x).ToList().ForEach(x => myParts.Add(x));

            return new ComplexPart(string.Empty, levels.Last().ToArray());
        }

        private List<List<IPart>> CreateLevels(int levelCount, MinMaxPair levelDiversity, MinMaxPair childRange)
        {
            var levels = new List<List<IPart>>();
            for (var levelIndex = 0; levelIndex < levelCount; levelIndex++)
            {
                var level = new List<IPart>();
                levels.Add(level);
                FillLevel(levels, levelIndex, levelDiversity, childRange);
            }

            return levels;
        }

        private static void RemoveUnusedParts(List<List<IPart>> levels)
        {
            var allParts = levels.SelectMany(x => x).ToList();
            levels.SkipLast(1).SelectMany(x => x)
                .Where(x => !allParts.Any(y => y.Parts.Contains(x))).ToList()
                .ForEach(x => levels.ForEach(l => l.Remove(x)));
        }

        private void AddRecursivity(List<List<IPart>> levels, double recursivityPercent, MinMaxPair recursiveJumpDistance)
        {
            foreach (var (level, levelIndex) in levels.Select((x, i) => (x, i)))
            {
                if (levels.Count - levelIndex < recursiveJumpDistance.Min)
                {
                    break;
                }

                foreach (var part in level)
                {
                    if (myRandom.NextDouble() > recursivityPercent)
                    {
                        continue;
                    }

                    var jumpDistance = GetRandomFromRange(recursiveJumpDistance, levels.Count - levelIndex);
                    var targetChild = GetNthParent(part, jumpDistance);
                    if (targetChild != null)
                    {
                        part.Parts.Add(targetChild);
                    }
                }
            }
        }

        private IPart? GetNthParent(IPart rootPart, int targetLevel)
        {
            if (targetLevel <= 0) { return rootPart; }

            var stack = new Stack<(IPart part, int leve)>();
            stack.Push((rootPart, targetLevel));
            while (stack.Count > 0)
            {
                var (part, level) = stack.Pop();
                foreach (var nextPart in part.Users.Shuffle(myRandom))
                {
                    if (level == 1)
                    {
                        return nextPart;
                    }

                    stack.Push((nextPart, level - 1));
                }
            }

            return null;
        }

        private void FillLevel(List<List<IPart>> levels, int levelIndex, MinMaxPair levelDiversity, MinMaxPair childRange)
        {
            var level = levels[levelIndex];
            var levelCount = GetRandomFromRange(levelDiversity);
            for (var levelChildIndex = 0; levelChildIndex < levelCount; levelChildIndex++)
            {
                var length = GetRandomFromRange(myConfig.WordLength);
                var word = GenerateWord(length);
                var part = new ComplexPart(word);

                var childCount = GetRandomFromRange(childRange);
                if (levelIndex > 0)
                {
                    var prevLevel = levels[levelIndex - 1];
                    for (var childIndex = 0; childIndex < childCount; childIndex++)
                    {
                        var childPart = prevLevel[myRandom.Next(prevLevel.Count)];
                        part.Parts.Add(childPart);
                        ((ComplexPart)childPart).Users.Add(part);
                    }
                }

                level.Add(part);
            }
        }

        private HashSet<IPart> myParts = new HashSet<IPart>();
    }
}
