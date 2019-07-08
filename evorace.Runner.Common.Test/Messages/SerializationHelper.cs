using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using evorace.Runner.Common.Messages;

namespace evorace.Runner.Common.Test.Messages
{
    internal static class SerializationHelper
    {
        public static IMessage SerializeAndDeserialize(AbstractMessage message)
        {
            var formatter = new BinaryFormatter();
            IMessage deserialized;
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, message);
                stream.Seek(0, SeekOrigin.Begin);
                deserialized = (IMessage)formatter.Deserialize(stream);
            }

            return deserialized;
        }
    }
}
