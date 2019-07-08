using evorace.Runner.Common.Messages;
using NUnit.Framework;

namespace evorace.Runner.Common.Test.Messages
{
    public class AbstractMessageTest
    {
        [Test]
        public void Test_Deserialize()
        {
            var testMessage = new TestMessage { TestProperty = "test value" };
            var serialized = testMessage.ToString();
            var deserialized = AbstractMessage.Deserialize(serialized);

            // Abstract properties are deserialized.
            Assert.That(deserialized.MessageType, Is.EqualTo(testMessage.MessageType));
            Assert.That(deserialized.Id, Is.EqualTo(testMessage.Id));

            // Derived properties are deserialzied.
            Assert.That(((TestMessage)deserialized).TestProperty, Is.EqualTo(testMessage.TestProperty));
        }

        private sealed class TestMessage : AbstractMessage
        {
            public string TestProperty { get; set; }
        }
    }
}