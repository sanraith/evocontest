using System;

namespace evocontest.Runner.Common.Messages.Request
{
    /// <summary>
    /// Instructs the receiver to terminate itself.
    /// </summary>
    [Serializable]
    public sealed class TerminateMessage : AbstractMessage
    { }
}
