using System;

namespace PerformanceTest
{
    public static class EnumerableExtensions
    {
        public static TResult Transform<TSource, TResult>(this TSource item, Func<TSource, TResult> selector)
        {
            return selector(item);
        }
    }
}
