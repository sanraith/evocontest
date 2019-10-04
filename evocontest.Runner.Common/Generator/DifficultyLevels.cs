using System;

namespace evocontest.Runner.Common.Generator
{
    public class DifficultyLevels
    {
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

        public DifficultyLevels()
        {
            myConfigGenerators = new Action<InputGeneratorConfig>[]
            {
                c => { // 0
                    c.SentenceLength = new MinMaxPair(5, 10);
                    c.PhraseLength = new MinMaxPair(2, 3);
                    c.PhraseCount = new MinMaxPair(1, 3);
                    c.PhraseRepeatCount = new MinMaxPair(2, 3);
                    c.DecoyPhraseCount = new MinMaxPair(1, 1);
                    c.DecoyRepeatCount = new MinMaxPair(2, 2);
                },

                c => { // 1
                    c.SentenceLength = new MinMaxPair(5, 20);
                    c.PhraseLength = new MinMaxPair(3, 6);
                    c.PhraseCount = new MinMaxPair(2, 6);
                    c.PhraseRepeatCount = new MinMaxPair(2, 6);
                    c.DecoyPhraseCount = new MinMaxPair(1, 2);
                    c.DecoyRepeatCount = new MinMaxPair(2, 3);
                },

                c => { // 2
                    c.SentenceLength = new MinMaxPair(5, 20);
                    c.PhraseLength = new MinMaxPair(3, 8);
                    c.PhraseCount = new MinMaxPair(4, 10);
                    c.PhraseRepeatCount = new MinMaxPair(2, 8);
                    c.DecoyPhraseCount = new MinMaxPair(2, 6);
                    c.DecoyRepeatCount = new MinMaxPair(2, 4);
                },

                c => { // 3
                    c.SentenceLength = new MinMaxPair(10, 40);
                    c.PhraseLength = new MinMaxPair(4, 10);
                    c.PhraseCount = new MinMaxPair(10, 20);
                    c.PhraseRepeatCount = new MinMaxPair(6, 10);
                    c.DecoyPhraseCount = new MinMaxPair(2, 10);
                    c.DecoyRepeatCount = new MinMaxPair(2, 6);
                },

                c => { // 4
                    c.SentenceLength = new MinMaxPair(30, 100);
                    c.PhraseLength = new MinMaxPair(5, 12);
                    c.PhraseCount = new MinMaxPair(12, 25);
                    c.PhraseRepeatCount = new MinMaxPair(6, 14);
                    c.DecoyPhraseCount = new MinMaxPair(5, 15);
                    c.DecoyRepeatCount = new MinMaxPair(3, 8);
                },

                c => { // 5
                    c.SentenceLength = new MinMaxPair(50, 160);
                    c.PhraseLength = new MinMaxPair(7, 15);
                    c.PhraseCount = new MinMaxPair(18, 32);
                    c.PhraseRepeatCount = new MinMaxPair(6, 16);
                    c.DecoyPhraseCount = new MinMaxPair(10, 25);
                    c.DecoyRepeatCount = new MinMaxPair(4, 10);
                },

                c => { // 6
                    c.SentenceLength = new MinMaxPair(60, 200);
                    c.PhraseLength = new MinMaxPair(10, 20);
                    c.PhraseCount = new MinMaxPair(55, 70);
                    c.PhraseRepeatCount = new MinMaxPair(8, 18);
                    c.DecoyPhraseCount = new MinMaxPair(55, 65);
                    c.DecoyRepeatCount = new MinMaxPair(7, 15);
                },

                c => { // 7
                    c.SentenceLength = new MinMaxPair(80, 300);
                    c.PhraseLength = new MinMaxPair(12, 26);
                    c.PhraseCount = new MinMaxPair(75, 100);
                    c.PhraseRepeatCount = new MinMaxPair(10, 20);
                    c.DecoyPhraseCount = new MinMaxPair(65, 75);
                    c.DecoyRepeatCount = new MinMaxPair(7, 15);
                },

                c => { // 8
                    c.SentenceLength = new MinMaxPair(300, 1000);
                    c.PhraseLength = new MinMaxPair(14, 30);
                    c.PhraseCount = new MinMaxPair(140, 180);
                    c.PhraseRepeatCount = new MinMaxPair(14, 24);
                    c.DecoyPhraseCount = new MinMaxPair(110, 140);
                    c.DecoyRepeatCount = new MinMaxPair(9, 17);
                },

                c => { // 9
                    c.SentenceLength = new MinMaxPair(500, 1500);
                    c.PhraseLength = new MinMaxPair(14, 34);
                    c.PhraseCount = new MinMaxPair(220, 240);
                    c.PhraseRepeatCount = new MinMaxPair(16, 30);
                    c.DecoyPhraseCount = new MinMaxPair(180, 230);
                    c.DecoyRepeatCount = new MinMaxPair(9, 22);
                },

                c => { // 10
                    c.SentenceLength = new MinMaxPair(800, 2200);
                    c.PhraseLength = new MinMaxPair(17, 40);
                    c.PhraseCount = new MinMaxPair(270, 320);
                    c.PhraseRepeatCount = new MinMaxPair(16, 32);
                    c.DecoyPhraseCount = new MinMaxPair(230, 280);
                    c.DecoyRepeatCount = new MinMaxPair(9, 24);
                },

                c => { // 11
                    c.SentenceLength = new MinMaxPair(900, 2800);
                    c.PhraseLength = new MinMaxPair(20, 50);
                    c.PhraseCount = new MinMaxPair(430, 490);
                    c.PhraseRepeatCount = new MinMaxPair(16, 38);
                    c.DecoyPhraseCount = new MinMaxPair(250, 330);
                    c.DecoyRepeatCount = new MinMaxPair(7, 18);
                },

                c => { // 12
                    c.SentenceLength = new MinMaxPair(1500, 5000);
                    c.PhraseLength = new MinMaxPair(14, 55);
                    c.PhraseCount = new MinMaxPair(750, 800);
                    c.PhraseRepeatCount = new MinMaxPair(12, 45);
                    c.DecoyPhraseCount = new MinMaxPair(350, 450);
                    c.DecoyRepeatCount = new MinMaxPair(6, 22);
                },

                c => { // 13
                    c.SentenceLength = new MinMaxPair(4000, 12000);
                    c.PhraseLength = new MinMaxPair(14, 70);
                    c.PhraseCount = new MinMaxPair(1100, 1300);
                    c.PhraseRepeatCount = new MinMaxPair(15, 70);
                    c.DecoyPhraseCount = new MinMaxPair(450, 550);
                    c.DecoyRepeatCount = new MinMaxPair(6, 24);
                },

                c => { // 14
                    c.SentenceLength = new MinMaxPair(7000, 20000);
                    c.PhraseLength = new MinMaxPair(20, 100);
                    c.PhraseCount = new MinMaxPair(3000, 3200);
                    c.PhraseRepeatCount = new MinMaxPair(45, 100);
                    c.DecoyPhraseCount = new MinMaxPair(950, 1050);
                    c.DecoyRepeatCount = new MinMaxPair(16, 45);
                },

                c => { // 15
                    c.SentenceLength = new MinMaxPair(10000, 50000);
                    c.PhraseLength = new MinMaxPair(30, 150);
                    c.PhraseCount = new MinMaxPair(3700, 4000);
                    c.PhraseRepeatCount = new MinMaxPair(65, 140);
                    c.DecoyPhraseCount = new MinMaxPair(1350, 1650);
                    c.DecoyRepeatCount = new MinMaxPair(20, 65);
                },
            };
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

            return config;
        }

        private readonly Action<InputGeneratorConfig>[] myConfigGenerators;
    }
}
