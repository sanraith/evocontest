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
            var difficultyLevels = new DifficultyLevels();
            for (int i = 0; i < count; i++)
            {
                var config = difficultyLevels.GetConfig(myRandom.Next(), difficultyLevel);
                var generator = new InputGenerator(config);
                yield return generator.Generate();
            }
        }

        private readonly Random myRandom;
    }
}
