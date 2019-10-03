using System;

namespace evocontest.Runner.Host.Common.Messages
{
    public interface IMessage
    {
        Guid Id { get; }
    }
}
