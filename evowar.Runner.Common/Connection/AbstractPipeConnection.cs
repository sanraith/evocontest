using evowar.Runner.Common.Messages;
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

        public async Task<IMessage> ReceiveMessageAsync()
        {
            using var reader = new StreamReader(Stream);
            var json = await reader.ReadLineAsync();
            var command = AbstractMessage.Deserialize(json);

            return command;
        }

        public async Task SendMessageAsync<TMessage>(TMessage message) where TMessage : IMessage
        {
            using var writer = new StreamWriter(Stream);
            var json = message.ToString();
            await writer.WriteLineAsync(json);
        }

        public virtual void Dispose()
        {
            Stream.Dispose();
        }
    }
}
