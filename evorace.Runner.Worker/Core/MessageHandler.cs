using evorace.Common;
using evorace.Runner.Common.Messages;
using evorace.Runner.Common.Utility;
using System;
using System.Linq;
using System.Reflection;

namespace evorace.Runner.Worker.Core
{
    public sealed class MessageHandler : IDisposable
    {
        public MessageHandlerResult Handle(IMessage message)
        {
            switch (message)
            {
                case LoadContextMessage loadMsg:
                    return HandleLoadContextMessage(loadMsg);

                case RunUnitTestsMessage runMsg:
                    return HandleRunUnitTestMessage(runMsg);

                case TerminateMessage _:
                    return new MessageHandlerResult(isDone: true);

                default:
                    throw new InvalidOperationException("Unknown message type!");
            }
        }

        public void Dispose()
        {
            ClearLoadedAssembly();
        }

        private MessageHandlerResult HandleLoadContextMessage(LoadContextMessage loadMsg)
        {
            Console.WriteLine(loadMsg.TargetAssemblyPath);
            IMessage response;
            try
            {
                if (myLoadedSolutionType != null) { ClearLoadedAssembly(); }

                var loadedAssembly = AssemblyLoader.Load(loadMsg.TargetAssemblyPath);
                var solutionType = loadedAssembly.Value.GetTypes().Single(x => typeof(ISolution).IsAssignableFrom(x));
                myLoadedSolutionType = new DisposableValue<Type>(solutionType, null, loadedAssembly);

                response = new OperationSuccessfulMessage(loadMsg.Id);
            }
            catch (Exception e)
            {
                response = new OperationFailedMessage(loadMsg.Id, e.ToString());
            }

            return new MessageHandlerResult(response);
        }

        private MessageHandlerResult HandleRunUnitTestMessage(RunUnitTestsMessage runMsg)
        {
            if (myLoadedSolutionType == null)
            {
                return new MessageHandlerResult(new OperationFailedMessage(runMsg.Id, "No assembly is loaded."));
            }
            return new MessageHandlerResult(new OperationSuccessfulMessage(runMsg.Id)); //TODO
        }

        private void ClearLoadedAssembly()
        {
            myLoadedSolutionType?.Dispose();
            myLoadedSolutionType = null;
        }

        private DisposableValue<Type>? myLoadedSolutionType;
    }

    public sealed class MessageHandlerResult
    {
        public bool IsDone { get; }

        public IMessage? Response { get; }

        public MessageHandlerResult(IMessage? response = null, bool isDone = false)
        {
            Response = response;
            IsDone = isDone;
        }
    }
}
