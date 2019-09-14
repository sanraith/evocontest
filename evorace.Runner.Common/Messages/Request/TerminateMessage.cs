using System;

namespace evorace.Runner.Common.Messages.Request
{
    /// <summary>
    /// Instructs the receiver to terminate itself.
    /// </summary>
    [Serializable]
    public sealed class TerminateMessage : AbstractMessage
    { }
}
