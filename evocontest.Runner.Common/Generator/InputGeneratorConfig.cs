using System;
using System.Collections.Generic;
using System.Text;

namespace evocontest.Runner.Common.Generator
{
    public sealed class InputGeneratorConfig
    {
        public int Seed { get; set; }

        public int InputLength { get; set; }

        /// <summary>
        /// Word length in characters.
        /// </summary>
        public MinMaxPair WordLength { get; set; }

        /// <summary>
        /// Sentence length in characters.
        /// </summary>
        public MinMaxPair SentenceLength { get; set; }
    }
}
