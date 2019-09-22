using System;
using evocontest.Runner.Common.Messages;

namespace evocontest.Runner.Common.Test.Messages
{
    [Serializable]
    internal sealed class TestMessage : AbstractMessage
    {
        public string TestProperty { get; set; }
    }
}