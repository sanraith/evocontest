using System;
using System.Threading.Tasks;

namespace evorace.Runner.Host.Extensions
{
    public static class LoggerExtensions
    {
        public static async Task<TResult> LogProgress<TResult>(this Task<TResult> task, string message)
        {
            Console.Write($"{message}... ");
            var result = await task;
            Console.WriteLine("done.");
            return result;
        }

        public static Task LogProgress(this Task task, string message)
        {
            return task.ContinueWith(_ => Task.FromResult(true)).LogProgress(message);
        }

        public static TResult LogProgress<TResult>(string message, Func<TResult> func)
        {
            return Task.Run(func).LogProgress(message).GetAwaiter().GetResult();
        }

        public static void LogProgress(string message, Action action)
        {
            Task.Run(action).LogProgress(message).GetAwaiter().GetResult();
        }
    }
}
