using System;
using evorace.Runner.Common.Messages;

namespace evorace.Runner.Common.Test.Messages
{
    [Serializable]
    internal sealed class TestMessage : AbstractMessage
    {
        public string TestProperty { get; set; }
    }
}