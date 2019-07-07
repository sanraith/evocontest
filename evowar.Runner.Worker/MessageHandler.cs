using evowar.Runner.Common.Messages;
using System;

namespace evowar.Runner.Worker
{
    public sealed class MessageHandler
    {
        public MessageHandlerResult Handle(IMessage message)
        {
            switch (message)
            {
                case LoadContextMessage loadMsg:
                    return HandleLoadContextMessage(loadMsg);

                case TerminateMessage _:
                    return new MessageHandlerResult(isDone: true);

                default:
                    throw new InvalidOperationException("Unknown message type!");
            }
        }

        private static MessageHandlerResult HandleLoadContextMessage(LoadContextMessage loadMsg)
        {
            IMessage response;
            try
            {
                // TODO actually load assembly
                response = new OperationSuccessfulMessage(loadMsg.Id);
            }
            catch (Exception e)
            {
                response = new OperationFailedMessage(loadMsg.Id, e.ToString());
            }

            return new MessageHandlerResult(response);
        }
    }

    public sealed class MessageHandlerResult
    {
        public bool IsDone { get; }

        public IMessage? Response { get; }

        public MessageHandlerResult(IMessage? response = null, bool isDone = false)
        {
            Response = response;
            IsDone = isDone;
        }
    }
}
