using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using evorace.Runner.Common.Connection;
using evorace.Runner.Common.Messages;
using evorace.Runner.Common.Test.Messages;
using NUnit.Framework;

namespace evorace.Runner.Common.Test.Connection
{
    [TestFixture]
    internal sealed class PipeServerClientTest
    {
        [Test]
        public void Test_SendReceive_MessageContent()
        {
            var sendMessage = new TestMessage { TestProperty = "testValue" };
            var serverTask = Task.Run(async () =>
            {
                using var server = new PipeServer(PipeName);
                await server.WaitForConnectionAsync();
                server.SendMessage(sendMessage);
            });

            IMessage receivedMessage = null;
            var clientTask = Task.Run(async () =>
            {
                using var client = new PipeClient(PipeName);
                await client.ConnectAsync();
                receivedMessage = client.ReceiveMessage();
            });
            var tasksCompleted = Task.WaitAll(new[] { serverTask, clientTask }, TimeSpan.FromSeconds(1));

            Assert.That(tasksCompleted, Is.True);
            Assert.That(receivedMessage.Id, Is.EqualTo(sendMessage.Id));
            Assert.That(receivedMessage, Is.InstanceOf<TestMessage>());
            Assert.That(((TestMessage)receivedMessage).TestProperty, Is.EqualTo(sendMessage.TestProperty));
        }

        [Test]
        public void Test_SendReceive_MessagesBlock()
        {
            const int messageCount = 10;
            var sentValues = Enumerable.Range(0, messageCount).Select(x => $"value #{x}").ToList();
            using var barrier = new Barrier(2);

            var serverTask = Task.Run(async () =>
            {
                using var server = new PipeServer(PipeName);
                await server.WaitForConnectionAsync();
                sentValues.ForEach(x => server.SendMessage(new TestMessage { TestProperty = x }));
                barrier.SignalAndWait();
            });

            List<string> receivedValues = new List<string>();
            var clientTask = Task.Run(async () =>
            {
                using var client = new PipeClient(PipeName);
                await client.ConnectAsync();
                barrier.SignalAndWait();

                for (int i = 0; i < messageCount; i++)
                {
                    receivedValues.Add(((TestMessage)client.ReceiveMessage()).TestProperty);
                }
            });
            var tasksCompleted = Task.WaitAll(new[] { serverTask, clientTask }, TimeSpan.FromSeconds(1));

            Assert.That(tasksCompleted, Is.False);
            Assert.That(receivedValues, Is.EqualTo(new string[0]));
        }

        private const string PipeName = "testPipe";
    }
}
