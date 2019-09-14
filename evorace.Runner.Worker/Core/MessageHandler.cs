using evorace.Common;
using evorace.Runner.Common.Messages;
using evorace.Runner.Common.Messages.Request;
using evorace.Runner.Common.Messages.Response;
using evorace.Runner.Common.Utility;
using System;
using System.Linq;

namespace evorace.Runner.Worker.Core
{
    public sealed class MessageHandler : IDisposable
    {
        public MessageHandlerResult Handle(IMessage message)
        {
            return message switch
            {
                LoadContextMessage loadMsg => HandleUnknownErrors(loadMsg, HandleLoadContextMessage),
                RunUnitTestsMessage runMsg => HandleUnknownErrors(runMsg, HandleRunUnitTestMessage),
                TerminateMessage _ => new MessageHandlerResult(isDone: true),
                _ => throw new InvalidOperationException("Unknown message type!"),
            };
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

            var testRunner = new UnitTestRunner(myLoadedSolutionType.Value);
            var testResults = testRunner.RunTests();

            return new MessageHandlerResult(new UnitTestResultMessage(testResults));
        }

        private static MessageHandlerResult HandleUnknownErrors<TOriginalMessage>(TOriginalMessage message, Func<TOriginalMessage, MessageHandlerResult> originalHandler)
            where TOriginalMessage : IMessage
        {
            try
            {
                return originalHandler(message);
            }
            catch (Exception ex)
            {
                return new MessageHandlerResult(new OperationFailedMessage(message.Id, $"Unhandled exception: {ex}"));
            }
        }

        private void ClearLoadedAssembly()
        {
            myLoadedSolutionType?.Dispose();
            myLoadedSolutionType = null;
        }

        private DisposableValue<Type>? myLoadedSolutionType;
    }
}
