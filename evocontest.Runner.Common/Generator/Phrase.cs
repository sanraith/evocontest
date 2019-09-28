using System.Collections.Generic;
using System.Linq;

namespace evocontest.Runner.Common.Generator
{
    public abstract class Phrase
    {
        public IReadOnlyList<string> Words { get; }

        public string Acronym { get; }

        public Phrase(IEnumerable<string> words)
        {
            Words = words.ToList();
            Acronym = Words.Count > 1 ? string.Concat(Words.Select(x => x[0])).ToUpper() : Words[0];
            myHashCode = Words.Aggregate(0, (sum, word) => sum ^ word.GetHashCode());
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

    public sealed class SinglePhrase : Phrase
    {
        public SinglePhrase(string word) : base(new[] { word })
        { }
    }
}
