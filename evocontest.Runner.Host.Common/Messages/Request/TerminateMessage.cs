using System;

namespace evocontest.Runner.Host.Common.Messages.Request
{
    /// <summary>
    /// Instructs the receiver to terminate itself.
    /// </summary>
    [Serializable]
    public sealed class TerminateMessage : AbstractMessage
    { }
}
