using System;

namespace evorace.Runner.Common.Messages
{
    /// <summary>
    /// Informs the receiver that the handling of the target message failed.
    /// </summary>
    [Serializable]
    public sealed class OperationFailedMessage : AbstractMessage
    {
        public Guid TargetMessageId { get; set; }

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
