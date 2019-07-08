using NUnit.Framework;

namespace evorace.Runner.Common.Test.Messages
{
    public partial class AbstractMessageTest
    {
        [Test]
        public void Test_Serializable()
        {
            var testMessage = new TestMessage { TestProperty = "test value" };
            var deserialized = SerializationHelper.SerializeAndDeserialize(testMessage);

            // Abstract properties are deserialized.
            Assert.That(deserialized.MessageType, Is.EqualTo(testMessage.MessageType));
            Assert.That(deserialized.Id, Is.EqualTo(testMessage.Id));

            // Derived properties are deserialzied.
            Assert.That(((TestMessage)deserialized).TestProperty, Is.EqualTo(testMessage.TestProperty));
        }
    }
}