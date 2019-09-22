using System;

namespace evocontest.Runner.Common.Messages
{
    [Serializable]
    public abstract class AbstractMessage : IMessage
    {
        public Guid Id { get; set; }

        public string MessageType { get; }

        public AbstractMessage()
        {
            MessageType = GetType().FullName!;
            Id = Guid.NewGuid();
        }

        public override string ToString()
        {
            return $"{Id.ToString()[..6]} {GetType().Name}";
        }
    }
}
