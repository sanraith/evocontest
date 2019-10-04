using System.Collections.Generic;
using System.Linq;

namespace evocontest.Runner.Common.Generator
{
    /// <summary>
    /// Represents a phrase.
    /// </summary>
    public abstract class Phrase
    {
        public IReadOnlyList<string> Words { get; }

        public string Acronym { get; }

        public int Length { get; }

        public Phrase(IEnumerable<string> words)
        {
            Words = words.ToList();
            Acronym = GetAcronym(Words);
            myHashCode = Words.Aggregate(0, (sum, word) => sum ^ word.GetHashCode());
            Length = Words.Sum(x => x.Length) + Words.Count - 1;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Phrase other)
            {
                return Words.SequenceEqual(other.Words);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return myHashCode;
        }
        
        public static string GetAcronym(IReadOnlyList<string> words)
        {
            return words.Count == 1 && words[0].All(char.IsUpper) ? words[0] : string.Concat(words.Select(x => x[0])).ToUpper();
        }

        public static string GetSingleWordOrAcronym(IReadOnlyList<string> words)
        {
            return words.Count > 1 ? string.Concat(words.Select(x => x[0])).ToUpper() : words[0];
        }

        private readonly int myHashCode;
    }

    public sealed class NormalPhrase : Phrase
    {
        public NormalPhrase(IEnumerable<string> words) : base(words)
        { }
    }

    public sealed class DecoyPhrase : Phrase
    {
        public DecoyPhrase(IEnumerable<string> words) : base(words)
        { }
    }

    public sealed class JunkPhrase : Phrase
    {
        public JunkPhrase(IEnumerable<string> words) : base(words)
        { }
    }
}
