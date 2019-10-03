using System.IO.Pipes;
using System.Threading.Tasks;

namespace evocontest.Runner.Host.Common.Connection
{
    public sealed class PipeServer : AbstractPipeConnection<NamedPipeServerStream>
    {
        public PipeServer(string name) : base(new NamedPipeServerStream(name, PipeDirection.InOut, 1))
        { }

        public async Task WaitForConnectionAsync()
        {
            await Stream.WaitForConnectionAsync();
        }
    }
}
