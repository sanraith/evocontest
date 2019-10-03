using System;
using System.Collections.Generic;
using System.Linq;

namespace evocontest.Runner.Common.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Creates (item, index) pairs for each element of the sequence.
        /// </summary>
        public static IEnumerable<(TSource Item, int Index)> WithIndex<TSource>(this IEnumerable<TSource> sequence)
        {
            return sequence.Select((item, index) => (Item: item, Index: index));
        }

        /// <summary>
        /// Shuffles the specified sequence using the given random number generator.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the given sequence.</typeparam>
        /// <param name="sequence">The sequence to transform.</param>
        /// <param name="random">The random number generator.</param>
        /// <returns>The shuffled sequence.</returns>
        public static IEnumerable<TSource> Shuffle<TSource>(this IEnumerable<TSource> sequence, Random random)
        {
            return sequence.ShuffleIterator(random);
        }

        /// <summary>
        /// Shuffles the specified list by reordering the values within the same instance.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the given list.</typeparam>
        /// <param name="list">The sequence to transform.</param>
        /// <param name="random">The random number generator.</param>
        /// <returns>The original instance, shuffled.</returns>
        public static IList<TSource> ShuffleInPlace<TSource>(this IList<TSource> list, Random random)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = random.Next(i, list.Count);
                (list[j], list[i]) = (list[i], list[j]);
            }
            return list;
        }

        private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Random rng)
        {
            var buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
    }
}
