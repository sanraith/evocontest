using System;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace evowar.Runner.Common.Messages
{
    public abstract class AbstractMessage : IMessage
    {
        public Guid Id { get; private set; }

        public string MessageType { get; private set; }

        public AbstractMessage()
        {
            MessageType = GetType().FullName;
            Id = Guid.NewGuid();
        }

        public static IMessage Deserialize(string json)
        {
            var commandTypeName = JsonSerializer.Parse<EmptyMessage>(json).MessageType;
            var type = TypeCache.GetOrAdd(commandTypeName, x => Type.GetType(x));
            var message = (IMessage)JsonSerializer.Parse(json, type);

            return message;
        }

        private struct EmptyMessage : IMessage
        {
            public Guid Id { get; }
            public string MessageType { get; set; }
        }

        private static readonly ConcurrentDictionary<string, Type> TypeCache = new ConcurrentDictionary<string, Type>();
    }
}
