using System;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace evowar.Runner.Common.Commands
{
    public abstract class AbstractCommand : ICommand
    {
        public string CommandType { get; }

        public AbstractCommand()
        {
            CommandType = GetType().FullName;
        }

        public static ICommand Deserialize(string json)
        {
            var commandTypeName = JsonSerializer.Parse<EmptyCommand>(json).CommandType;
            var type = TypeCache.GetOrAdd(commandTypeName, x => Type.GetType(x));
            var command = (ICommand)JsonSerializer.Parse(json, type);

            return command;
        }

        private struct EmptyCommand : ICommand
        {
            public string CommandType { get; set; }
        }

        private static readonly ConcurrentDictionary<string, Type> TypeCache = new ConcurrentDictionary<string, Type>();
    }
}
