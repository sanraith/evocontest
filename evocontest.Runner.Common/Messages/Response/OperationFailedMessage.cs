using System;

namespace evocontest.Runner.Common.Messages.Response
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
