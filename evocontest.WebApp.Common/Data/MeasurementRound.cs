using evocontest.Runner.Common.Generator;
using System;

namespace evocontest.WebApp.Common.Data
{
    public class MeasurementRoundContainer
    {
        public int DifficultyLevel { get; set; }

        public double TotalMilliseconds { get; set; }

        public MeasurementError? Error { get; set; }

        public double[] SplitMilliseconds { get; set; } = Array.Empty<double>();

        public InputGeneratorConfig[]? GeneratorConfigs { get; set; }
    }
}
