using System;

namespace evocontest.Runner.Common.Messages.Request
{
    [Serializable]
    public sealed class MeasureSolveMessage : AbstractMessage
    {
        public string Input { get; set; }

        private MeasureSolveMessage()
        {
            Input = null!;
        }

        public MeasureSolveMessage(string input)
        {
            Input = input;
        }
    }
}
