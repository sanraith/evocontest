﻿using System;

namespace evowar.Runner.Common.Messages
{
    public interface IMessage
    {
        Guid Id { get; }

        string MessageType { get; }
    }
}
