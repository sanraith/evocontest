using evocontest.Common;
using evocontest.Runner.Host.Common.Messages;
using evocontest.Runner.Host.Common.Messages.Request;
using evocontest.Runner.Host.Common.Messages.Response;
using evocontest.Runner.Host.Common.Utility;
using System;
using System.Diagnostics;
using System.Linq;

namespace evocontest.Runner.Worker.Core
{
    public sealed class MessageHandler : IDisposable
    {
        public MessageHandlerResult Handle(IMessage message)
        {
            return message switch
            {
                LoadContextMessage loadMsg => HandleUnknownErrors(loadMsg, HandleLoadContextMessage),
                RunUnitTestsMessage runMsg => HandleUnknownErrors(runMsg, HandleRunUnitTestMessage),
                MeasureSolveMessage solveMsg => HandleUnknownErrors(solveMsg, HandleMeasureSolveMessage),
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
                var solutionType = loadedAssembly.Value.GetTypes().First(x => typeof(ISolution).IsAssignableFrom(x));
                myLoadedSolutionType = DisposableValue.Create(solutionType, loadedAssembly);

                response = new OperationSuccessfulMessage(loadMsg.Id);
            }
            catch (Exception e)
            {
                response = new OperationFailedMessage(loadMsg.Id, e.ToString(), e);
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

        private MessageHandlerResult HandleMeasureSolveMessage(MeasureSolveMessage solveMsg)
        {
            if (myLoadedSolutionType == null)
            {
                return new MessageHandlerResult(new OperationFailedMessage(solveMsg.Id, "No assembly is loaded."));
            }

            var sw = Stopwatch.StartNew();
            var instance = (ISolution)Activator.CreateInstance(myLoadedSolutionType.Value)!;
            var output = instance.Solve(solveMsg.Input); // TODO throw away input string
            sw.Stop();

            return new MessageHandlerResult(new MeasureSolveResultMessage(output, sw.Elapsed));
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
                return new MessageHandlerResult(new OperationFailedMessage(message.Id, $"Unhandled exception: {ex}", ex));
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
