using evowar.Runner.Common.Messages;
using NUnit.Framework;

namespace evowar.Runner.Worker.Test
{
    public class MessageHandlerTests
    {
        [SetUp]
        public void Setup()
        {
            myMessageHandler = new MessageHandler();
        }

        [Test]
        public void TerminateMessageTest()
        {
            var result = myMessageHandler.Handle(new TerminateMessage());

            Assert.That(result.IsDone, Is.True);
            Assert.That(result.Response, Is.Null);
        }

        private MessageHandler myMessageHandler;
    }
}