using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace evocontest.Runner.Common.Generator
{
    public sealed class InputGeneratorManager
    {
        public List<GeneratorResult> Generate(int difficultyLevel, int count)
        {
            //var dummySentences = "aa bb cc dd. aa bb cc dd. aa BCD. ABC dd.";
            //var solvedDummySentences = "ABCD. ABCD. ABCD. ABCD.";
            //var totalLength = Math.Pow(2, difficultyLevel);
            //var repeatCount = (int)Math.Ceiling(totalLength / dummySentences.Length);
            //var dummyInput = string.Join(' ', Enumerable.Repeat(dummySentences, repeatCount));
            //var dummyOutput = string.Join(' ', Enumerable.Repeat(solvedDummySentences, repeatCount));

            //var results = new List<GeneratorResult>();
            //for (int i = 0; i < count; i++)
            //{
            //    results.Add(new GeneratorResult
            //    {
            //        Input = dummyInput,
            //        Solution = dummyOutput
            //    });
            //}

            //var generator = new FinalInputGenerator(new InputGeneratorConfig
            //{
            //    InputLength = 1024 * 1024 * 1024,
            //    SentenceLength = new MinMaxPair(5, 10)
            //});
            //var result = generator.Generate();

            var result = new GeneratorResult
            {
                Input = new string('a', 1024 * 1024 * 1024) + ".",
                Solution = new string('a', 1024 * 1024 * 1024) + ".",
            };

            return new List<GeneratorResult> { result };
        }
    }
}
