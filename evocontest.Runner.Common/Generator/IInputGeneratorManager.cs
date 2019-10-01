using System;
using System.Collections.Generic;
using System.Text;

namespace evocontest.Runner.Common.Generator
{
    public interface IInputGeneratorManager
    {
        public IEnumerable<GeneratorResult> Generate(int difficultyLevel, int count);
    }
}
