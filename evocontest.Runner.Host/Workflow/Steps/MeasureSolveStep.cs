using evocontest.Runner.Common.Generator;
using evocontest.Runner.Host.Common.Connection;
using evocontest.Runner.Host.Common.Messages;
using evocontest.Runner.Host.Common.Messages.Request;
using evocontest.Runner.Host.Core;
using System.Threading.Tasks;

namespace evocontest.Runner.Host.Workflow.Steps
{
    public sealed class MeasureSolveStep : IResolvable
    {
        public Task<IMessage> ExecuteAsync(PipeServer pipeServer, GeneratorResult generatorResult)
        {
            return Task.Run(() =>
            {
                pipeServer.SendMessage(new MeasureSolveMessage(generatorResult.Input));
                var response = pipeServer.ReceiveMessage();
                return response;
            });
        }
    }
}
