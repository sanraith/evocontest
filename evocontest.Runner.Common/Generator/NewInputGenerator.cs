using evocontest.Runner.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace evocontest.Runner.Common.Generator
{
    public sealed class NewInputGenerator : InputGeneratorBase
    {
        public NewInputGenerator(InputGeneratorConfig config) : base(config)
        { }


        public GeneratorResult Generate(int maxLevel)
        {
            var sw = Stopwatch.StartNew();

            List<string> words = GenerateSentence();

            var text = string.Join(" ", words) + ".";
            Console.WriteLine(text);

            return new GeneratorResult { Input = text };
        }

        private List<string> GenerateSentence()
        {
            var sentenceLength = GetRandomFromRange(myConfig.SentenceLength);
            var words = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                var length = GetRandomFromRange(myConfig.WordLength);
                var word = GenerateWord(length);
                words.Add(word);
            }
            words[0] = words[0][0..1].ToUpper() + words[0][1..];

            return words;
        }
    }
}
