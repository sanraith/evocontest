using System;
using System.Threading.Tasks;

namespace LoginTest
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
            return LogProgress(task.ContinueWith(_ => Task.FromResult(true)), message);
        }

        public static TResult LogProgress<TResult>(string message, Func<TResult> func)
        {
            return LogProgress(Task.Run(func), message).GetAwaiter().GetResult();
        }

        public static void LogProgress(string message, Action action)
        {
            LogProgress(Task.Run(action), message).GetAwaiter().GetResult();
        }
    }
}
