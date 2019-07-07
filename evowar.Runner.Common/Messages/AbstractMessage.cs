using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace evowar.Runner.Common.Messages
{
    public abstract class AbstractMessage : IMessage
    {
        public Guid Id { get; set; }

        public string MessageType { get; }

        public AbstractMessage()
        {
            MessageType = GetType().FullName;
            Id = Guid.NewGuid();
        }

        public override string ToString()
        {
            return JsonSerializer.ToString(this, GetType());
        }

        public static IMessage Deserialize(string json)
        {
            var typeName = JsonSerializer.Parse<EmptyMessage>(json).MessageType;
            var type = TypeCache.GetOrAdd(typeName, FindType);
            var message = (IMessage)JsonSerializer.Parse(json, type);

            return message;
        }

        private static Type FindType(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => AssemblyCache.GetOrAdd(a, assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch (Exception)
                    {
                        return new Type[0];
                    }
                })).FirstOrDefault(t => t.FullName.Equals(fullName));
        }

        private struct EmptyMessage : IMessage
        {
            public Guid Id { get; }
            public string MessageType { get; set; }
        }

        private static readonly ConcurrentDictionary<Assembly, Type[]> AssemblyCache = new ConcurrentDictionary<Assembly, Type[]>();
        private static readonly ConcurrentDictionary<string, Type> TypeCache = new ConcurrentDictionary<string, Type>();
    }
}
