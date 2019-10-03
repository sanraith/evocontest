namespace evocontest.Runner.Common.Generator
{
    public sealed class InputGeneratorConfig
    {
        public int Seed { get; set; } = 0;

        /// <summary>
        /// The target length of the input in characters
        /// </summary>
        public int InputLength { get; set; } = 256;

        /// <summary>
        /// Word length in characters.
        /// </summary>
        public MinMaxPair WordLength { get; set; } = new MinMaxPair(2, 10);

        /// <summary>
        /// Sentence length in phrases.
        /// </summary>
        public MinMaxPair SentenceLength { get; set; } = new MinMaxPair(50, 100);

        /// <summary>
        /// Chance to have part of the phrase already collapsed.
        /// </summary>
        public double PhraseCollapseChance { get; set; } = .2;

        /// <summary>
        /// Phrase length in words.
        /// </summary>
        public MinMaxPair PhraseLength { get; set; } = new MinMaxPair(2, 5);

        /// <summary>
        /// Number of distinct phrases.
        /// </summary>
        public MinMaxPair PhraseCount { get; set; } = new MinMaxPair(10, 20);

        /// <summary>
        /// Number of times a distinct phrase is repeated.
        /// </summary>
        public MinMaxPair PhraseRepeatCount { get; set; } = new MinMaxPair(2, 10);

        /// <summary>
        /// Number of distinct almost-matching phrases.
        /// </summary>
        public MinMaxPair DecoyPhraseCount { get; set; } = new MinMaxPair(5, 20);

        /// <summary>
        /// Number of times a decoy phrase is repeated.
        /// </summary>
        public MinMaxPair DecoyRepeatCount { get; set; } = new MinMaxPair(2, 5);
    }
}
