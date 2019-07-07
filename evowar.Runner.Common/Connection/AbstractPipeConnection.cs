using evowar.Runner.Common.Commands;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace evowar.Runner.Common.Connection
{
    public abstract class AbstractPipeConnection<TStream> : IDisposable where TStream : PipeStream
    {
        public TStream Stream { get; private set; }

        public AbstractPipeConnection(TStream namedPipeStream)
        {
            Stream = namedPipeStream;
        }

        public async Task<ICommand> ReceiveCommandAsync()
        {
            using var reader = new StreamReader(Stream);
            var json = await reader.ReadLineAsync();
            var command = AbstractCommand.Deserialize(json);

            return command;
        }

        public async Task SendCommandAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            using var writer = new StreamWriter(Stream);
            await writer.WriteLineAsync(JsonSerializer.ToString(command));
        }

        public virtual void Dispose()
        {
            Stream?.Dispose();
            Stream = null;
        }
    }
}
