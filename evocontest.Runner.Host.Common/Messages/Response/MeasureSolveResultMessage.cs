using System;
using System.Collections.Generic;
using System.Text;

namespace evocontest.Runner.Host.Common.Messages.Response
{
    [Serializable]
    public sealed class MeasureSolveResultMessage : AbstractMessage
    {
        public string Output { get; set; }

        public TimeSpan Time { get; set; }

        public MeasureSolveResultMessage()
        {
            Output = null!;
        }

        public MeasureSolveResultMessage(string output, TimeSpan time)
        {
            Output = output;
            Time = time;
        }
    }
}
