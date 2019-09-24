using evocontest.Runner.Common.Connection;
using evocontest.Runner.Common.Generator;
using evocontest.Runner.Common.Messages;
using evocontest.Runner.Common.Messages.Request;
using evocontest.Runner.Common.Messages.Response;
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
