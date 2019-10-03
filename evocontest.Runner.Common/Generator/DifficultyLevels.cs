using System;

namespace evocontest.Runner.Common.Generator
{
    public class DifficultyLevels
    {
        public DifficultyLevels()
        {
            myConfigGenerators = new Action<InputGeneratorConfig>[]
            {
                c =>
                {
                    c.SentenceLength = new MinMaxPair(5, 10);
                    c.PhraseLength = new MinMaxPair(2, 3);
                    c.PhraseCount = new MinMaxPair(1, 3);
                    c.PhraseRepeatCount = new MinMaxPair(2, 3);
                    c.DecoyPhraseCount = new MinMaxPair(1, 1);
                    c.DecoyRepeatCount = new MinMaxPair(2, 2);
                },


                c =>
                {
                    var length = c.InputLength;
                    c.WordLength = new MinMaxPair(6, 10);
                    c.SentenceLength = new MinMaxPair(5, Math.Max(10, length / 50));

                    c.PhraseLength = new MinMaxPair(2, Math.Max(10, length / 500));
                    c.PhraseCount = new MinMaxPair(Math.Max(1, length / 2000), Math.Max(1, length / 100));
                    c.PhraseRepeatCount = new MinMaxPair(2, Math.Max(10, length / 1000));

                    c.DecoyPhraseCount = new MinMaxPair(Math.Max(1, length / 2000), Math.Max(1, length / 200));
                    c.DecoyRepeatCount = new MinMaxPair(2, Math.Max(10, length / 10000));
                },
            };
        }

        public InputGeneratorConfig GetConfig(int seed, int difficultyLevel)
        {
            InputGeneratorConfig config;
            if (difficultyLevel >= myConfigGenerators.Length)
            {
                config = CreateScalingConfig(seed, difficultyLevel);
            }
            else
            {
                config = CreateDefaultConfig(seed, difficultyLevel);
                myConfigGenerators[difficultyLevel](config);
            }

            return config;
        }

        private InputGeneratorConfig CreateDefaultConfig(int seed, int difficultyLevel) => new InputGeneratorConfig()
        {
            Seed = seed,
            InputLength = (int)Math.Pow(2, difficultyLevel) * 256,
            WordLength = new MinMaxPair(6, 10),
            PhraseCollapseChance = .2,

            DecoyPhraseCount = null,
            DecoyRepeatCount = null,
            PhraseCount = null,
            PhraseLength = null,
            PhraseRepeatCount = null,
            SentenceLength = null
        };

        private InputGeneratorConfig CreateScalingConfig(int seed, int difficultyLevel)
        {
            var config = CreateDefaultConfig(seed, difficultyLevel);
            myConfigGenerators[^1](config);
            config.InputLength = (int)Math.Pow(2, difficultyLevel) * 256;

            //var generator = new InputGenerator(new InputGeneratorConfig
            //{
            //    Seed = myRandom.Next(),
            //    InputLength = length,

            //    WordLength = new MinMaxPair(6, 10),
            //    SentenceLength = new MinMaxPair(5, Math.Max(10, length / 50)),

            //    PhraseLength = new MinMaxPair(2, Math.Max(10, length / 500)),
            //    PhraseCount = new MinMaxPair(Math.Max(1, length / 2000), Math.Max(1, length / 100)),
            //    PhraseCollapseChance = .1,
            //    PhraseRepeatCount = new MinMaxPair(2, Math.Max(10, length / 1000)),

            //    DecoyPhraseCount = new MinMaxPair(Math.Max(1, length / 2000), Math.Max(1, length / 200)),
            //    DecoyRepeatCount = new MinMaxPair(2, Math.Max(10, length / 10000))
            //}

            return config;
        }

        private readonly Action<InputGeneratorConfig>[] myConfigGenerators;
    }
}
