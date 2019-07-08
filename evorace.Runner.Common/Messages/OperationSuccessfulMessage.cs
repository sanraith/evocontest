using System;

namespace evorace.Runner.Common.Messages
{
    /// <summary>
    /// Informs the receiver that the handling of the target message was successful.
    /// </summary>
    public sealed class OperationSuccessfulMessage : AbstractMessage
    {
        public Guid TargetMessageId { get; set; }

        private OperationSuccessfulMessage() { }

        public OperationSuccessfulMessage(Guid targetMessageId)
        {
            TargetMessageId = targetMessageId;
        }
    }
}
