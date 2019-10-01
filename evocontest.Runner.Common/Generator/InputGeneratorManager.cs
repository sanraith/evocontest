using System;
using System.Collections.Generic;
using System.Linq;

namespace evocontest.Runner.Common.Generator
{
    public sealed class InputGeneratorManager : IInputGeneratorManager
    {
        public InputGeneratorManager(int seed = 0)
        {
            myRandom = new Random(seed);
        }

        public IEnumerable<GeneratorResult> Generate(int difficultyLevel, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var length = (int)Math.Pow(2, difficultyLevel) * 128;

                // TODO too few acronym for low difficulty
                // TODO size increase not granual enough
                var generator = new InputGenerator(new InputGeneratorConfig
                {
                    Seed = myRandom.Next(),
                    InputLength = length,

                    WordLength = new MinMaxPair(6, 10),
                    SentenceLength = new MinMaxPair(5, Math.Max(10, length / 50)),

                    PhraseLength = new MinMaxPair(2, Math.Max(10, length / 500)),
                    PhraseCount = new MinMaxPair(Math.Max(1, length / 2000), Math.Max(1, length / 100)),
                    PhraseCollapseChance = .1,
                    PhraseRepeatCount = new MinMaxPair(2, Math.Max(10, length / 1000)),

                    DecoyPhraseCount = new MinMaxPair(Math.Max(1, length / 2000), Math.Max(1, length / 200)),
                    DecoyRepeatCount = new MinMaxPair(2, Math.Max(10, length / 10000))
                });

                yield return generator.Generate();
            }
        }

        private readonly Random myRandom;
    }
}
