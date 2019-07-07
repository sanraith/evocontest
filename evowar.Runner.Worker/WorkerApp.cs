using evowar.Runner.Common.Connection;
using System.Threading.Tasks;

namespace evowar.Runner.Worker
{
    public sealed class WorkerApp
    {
        public const string PipeName = "evoWar.Runner";

        static async Task Main(string[] args)
        {
            await new WorkerApp().Run();
        }

        public async Task Run()
        {
            using var pipeClient = new PipeClient(PipeName);
            await pipeClient.ConnectAsync();

            var messageHandler = new MessageHandler();

            var isDone = false;
            while (!isDone)
            {
                var message = await pipeClient.ReceiveMessageAsync();
                var result = messageHandler.Handle(message);

                if (result.Response != null)
                {
                    await pipeClient.SendMessageAsync(result.Response);
                }

                isDone = result.IsDone;
            }
        }
    }
}
