using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerformanceTest
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> Middle<TSource>(this IReadOnlyList<TSource> list, int cutCount)
        {
            return list.Skip(cutCount).Take(list.Count - cutCount * 2);
        }

        public static TResult Select<TSource, TResult>(this TSource item, Func<TSource, TResult> selector)
        {
            return selector(item);
        }
    }
}
