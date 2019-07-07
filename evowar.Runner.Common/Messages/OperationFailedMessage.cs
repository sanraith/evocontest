using System;

namespace evowar.Runner.Common.Messages
{
    /// <summary>
    /// Informs the receiver that the handling of the target message failed.
    /// </summary>
    public sealed class OperationFailedMessage : AbstractMessage
    {
        public Guid TargetMessageId { get; private set; }

        public string? ErrorMessage { get; set; }

        private OperationFailedMessage()
        {
            ErrorMessage = null!;
        }

        public OperationFailedMessage(Guid targetMessageId, string errorMessage)
        {
            TargetMessageId = targetMessageId;
            ErrorMessage = errorMessage;
        }
    }
}
