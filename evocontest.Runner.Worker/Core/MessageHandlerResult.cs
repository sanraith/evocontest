using evocontest.Runner.Common.Messages;

namespace evocontest.Runner.Worker.Core
{
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
