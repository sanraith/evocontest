using System;

namespace evocontest.Runner.Host.Common.Messages.Response
{
    /// <summary>
    /// Informs the receiver that the handling of the target message failed.
    /// </summary>
    [Serializable]
    public sealed class OperationFailedMessage : AbstractMessage
    {
        public Guid TargetMessageId { get; set; }

        public string ErrorMessage { get; set; }

        public Exception Exception { get; set; }

        private OperationFailedMessage()
        {
            ErrorMessage = null!;
        }

        public OperationFailedMessage(Guid targetMessageId, string errorMessage, Exception exception = null)
        {
            TargetMessageId = targetMessageId;
            ErrorMessage = errorMessage;
            Exception = exception;
        }
    }
}
