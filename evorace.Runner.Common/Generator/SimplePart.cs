using System;
using System.Collections.Generic;

namespace evorace.Runner.Common.Generator
{

    public class SimplePart : IPart
    {
        public string ShortHand { get; }

        public List<IPart> Parts { get; } = new List<IPart>();

        public SimplePart(string text) => ShortHand = text;

        public int RenderTo(Span<char> span, int maxLevel) => ((IPart)this).RenderShortTo(span);

        public int GetLength(int level) => ShortHand.Length;
    }
}
