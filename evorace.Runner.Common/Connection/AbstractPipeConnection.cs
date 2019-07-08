using evorace.Runner.Common.Messages;
using System;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;

namespace evorace.Runner.Common.Connection
{
    public abstract class AbstractPipeConnection<TStream> : IDisposable where TStream : PipeStream
    {
        public TStream Stream { get; private set; }

        public AbstractPipeConnection(TStream namedPipeStream)
        {
            Stream = namedPipeStream;
        }

        public IMessage ReceiveMessage()
        {
            var command = (IMessage)myFormatter.Deserialize(Stream);

            return command;
        }

        public void SendMessage<TMessage>(TMessage message) where TMessage : IMessage
        {
            myFormatter.Serialize(Stream, message);
        }

        public virtual void Dispose()
        {
            Stream.Dispose();
        }

        private readonly BinaryFormatter myFormatter = new BinaryFormatter();
    }
}
