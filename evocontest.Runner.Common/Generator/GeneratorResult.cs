namespace evocontest.Runner.Common.Generator
{
    public sealed class GeneratorResult
    {
        public string Input { get; set; }

        public string Solution { get; set; }

        public InputGeneratorConfig Config { get; set; }

        public static GeneratorResult Empty => new GeneratorResult() { Input = string.Empty, Solution = string.Empty };
    }
}
