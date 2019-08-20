using System;
using System.Collections.Generic;
using System.Linq;

namespace evorace.Runner.Common.Extensions
{
    public static class EnumerableExtensions
    {
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
