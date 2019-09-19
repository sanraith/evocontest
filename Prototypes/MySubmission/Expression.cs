using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace MySubmission
{
    /// <summary>
    /// Represents several words forming an expression.
    /// </summary>
    public class Expression : IEquatable<Expression>
    {
        public IReadOnlyList<string> Words { get; }

        public string Acronym { get; }

        public Expression(IReadOnlyCollection<string> words)
        {
            Acronym = GenerateAcronym(words);
            Words = words.Select(x => x.ToLowerInvariant()).ToList();
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
            if (obj is Expression exp) { return Equals(exp); }
            return base.Equals(obj);
        }

        public override int GetHashCode() => myHashCode;
        
        public override string ToString() => string.Join(" ", Words);

        #endregion

        private static string GenerateAcronym(IEnumerable<string> words)
        {
            var sb = new StringBuilder();
            foreach (var word in words)
            {
                if (IsAcronym(word))
                {
                    sb.Append(word);
                }
                else
                {
                    sb.Append(char.ToUpper(word[0]));
                }
            }
            return sb.ToString();
        }

        private static bool IsAcronym(string word) => word.All(char.IsUpper);

        private readonly int myHashCode;
    }
}
