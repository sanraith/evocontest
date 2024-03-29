using evocontest.Runner.Host.Common.Messages.Request;
using evocontest.Runner.Worker.Core;
using NUnit.Framework;

namespace evocontest.Runner.Worker.Test
{
    public class MessageHandlerTests
    {
        [SetUp]
        public void Setup()
        {
            myMessageHandler = new MessageHandler();
        }

        [Test]
        public void Test_Handle_TerminateMessage()
        {
            var result = myMessageHandler.Handle(new TerminateMessage());

            Assert.That(result.IsDone, Is.True);
            Assert.That(result.Response, Is.Null);
        }

        private MessageHandler myMessageHandler;
    }
}