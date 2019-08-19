using System;
using System.Collections.Generic;

namespace evorace.Runner.Common.Generator
{

    public interface IPart
    {
        int GetLength(int level);

        int RenderTo(Span<char> span, int maxLevel);

        int RenderShortTo(Span<char> span)
        {
            ShortHand.AsSpan().CopyTo(span);
            return ShortHand.Length;
        }

        List<IPart> Parts { get; }

        string ShortHand { get; }

        string Render() => Render(int.MaxValue);

        string Render(int maxLevel)
        {
            var partSpan = new char[GetLength(maxLevel)].AsSpan();
            RenderTo(partSpan, maxLevel);
            return new string(partSpan);
        }
    }
}
