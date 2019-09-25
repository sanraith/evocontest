﻿using System;
using System.Collections.Generic;
using System.Text;

namespace evocontest.Runner.Common.Generator
{
    public sealed class GeneratorResult
    {
        public string Input { get; set; }

        public string Output { get; set; }

        public static GeneratorResult Empty => new GeneratorResult() { Input = string.Empty, Output = string.Empty };
    }
}
