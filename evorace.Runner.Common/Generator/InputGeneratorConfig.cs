using System;
using System.Collections.Generic;
using System.Text;

namespace evorace.Runner.Common.Generator
{
    public sealed class InputGeneratorConfig
    {
        public int Seed { get; set; }

        public int InputLength { get; set; }

        public MinMaxPair WordLength { get; set; }

        public MinMaxPair SentenceLength { get; set; }
    }
}
