using System;

namespace evowar.Runner.Common.Messages
{
    /// <summary>
    /// Informs the receiver that the handling of the target message was successful.
    /// </summary>
    public sealed class OperationSuccessfulMessage : AbstractMessage
    {
        public Guid TargetMessageId { get; private set; }

        private OperationSuccessfulMessage() { }

        public OperationSuccessfulMessage(Guid targetMessageId)
        {
            TargetMessageId = targetMessageId;
        }
    }
}
