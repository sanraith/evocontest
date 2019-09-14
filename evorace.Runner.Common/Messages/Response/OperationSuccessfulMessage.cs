using System;

namespace evorace.Runner.Common.Messages.Response
{
    /// <summary>
    /// Informs the receiver that the handling of the target message was successful.
    /// </summary>
    [Serializable]
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
