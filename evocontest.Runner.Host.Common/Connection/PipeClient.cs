﻿using evocontest.Runner.Host.Common.Connection;
using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace evocontest.Runner.Common.Connection
{
    public sealed class PipeClient : AbstractPipeConnection<NamedPipeClientStream>
    {
        public PipeClient(string name) : base(new NamedPipeClientStream(".", name, PipeDirection.InOut))
        { }

        public async Task ConnectAsync()
        {
            await Stream.ConnectAsync();
        }
    }
}
