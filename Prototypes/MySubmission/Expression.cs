using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MySubmission
{
    /// <summary>
    /// Represents several words forming an expression.
    /// </summary>
    public class Expression : IEquatable<Expression>
    {
        public IReadOnlyList<string> Words { get; }

        public string Acronym { get; }

        public Expression(IEnumerable<string> words)
        {
            Words = words.Select(x => x.ToLowerInvariant()).ToList();
            Acronym = string.Concat(Words.Select(x => x[0..1])).ToUpperInvariant();
            myHashCode = Words.Aggregate(0, (sum, word) => sum ^ word.GetHashCode());
        }

        #region IEquatable, ToString overrides

        public bool Equals([AllowNull] Expression other)
        {
            if (other == null) { return false; }
            return Words.SequenceEqual(other.Words);
        }

        public override bool Equals(object obj)
        {
            if (obj is Expression exp)
            {
                return Equals(exp);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return myHashCode;
        }


        public override string ToString()
        {
            return string.Join(" ", Words);
        }

        #endregion

        private readonly int myHashCode;
    }
}
