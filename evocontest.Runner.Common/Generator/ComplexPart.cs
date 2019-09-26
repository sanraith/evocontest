using System;
using System.Collections.Generic;
using System.Linq;

namespace evocontest.Runner.Common.Generator
{
    public class ComplexPart : IPart
    {
        public string ShortHand { get; }

        public HashSet<IPart> Users { get; }

        public List<IPart> Parts { get; }

        public ComplexPart(string shortHand, params IPart[] parts)
        {
            ShortHand = shortHand;
            Parts = parts.ToList();
            Users = new HashSet<IPart>();
        }

        public int GetLength(int maxLevel)
        {
            int length = 0;
            var queue = new Queue<(IPart part, int level)>();
            queue.Enqueue((this, maxLevel));

            while (queue.Count > 0)
            {
                var (part, level) = queue.Dequeue();
                length += Math.Max(0, part.Parts.Count - 1); // spaces

                if (part.Parts.Count == 0)
                {
                    length += part.ShortHand.Length;
                }
                else if (level == 0)
                {
                    foreach (var nextPart in part.Parts)
                    {
                        length += nextPart.ShortHand.Length;
                    }
                }
                else
                {
                    foreach (var nextPart in part.Parts)
                    {
                        queue.Enqueue((nextPart, level - 1));
                    }
                }
            }

            return length;
        }

        public int RenderTo(Span<char> span, int maxLevel)
        {
            var stack = new Stack<(IPart Part, int Level)>();
            stack.Push((this, maxLevel));

            var pos = 0;
            while (stack.Count > 0)
            {
                var (part, level) = stack.Pop();

                if (part.Parts.Count == 0)
                {
                    if (pos > 0) { span[pos++] = ' '; }
                    if (!TryRenderShortTo(part, span[pos..], ref pos)) { return pos; }
                }
                else if (level == 0)
                {
                    if (pos > 0) { span[pos++] = ' '; }
                    for (var i = 0; i < part.Parts.Count; i++)
                    {
                        if (i != 0) { span[pos++] = ' '; }
                        if (!TryRenderShortTo(part.Parts[i], span[pos..], ref pos)) { return pos; }
                    }
                }
                else
                {
                    for (var i = part.Parts.Count - 1; i >= 0; i--)
                    {
                        stack.Push((part.Parts[i], level - 1));
                    }
                }
            }

            return pos;
        }

        private static bool TryRenderShortTo(IPart part, Span<char> span, ref int pos)
        {
            if (part.ShortHand.Length > span.Length)
            {
                return false;
            }
            pos += part.RenderShortTo(span);
            return true;
        }

        public override string? ToString()
        {
            return $"{ShortHand}({string.Join(' ', Parts.Select(x => x.ShortHand))})";
        }
    }
}
