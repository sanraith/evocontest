using System;
using System.Threading.Tasks;

namespace evorace.Runner.Host.Extensions
{
    public static class LoggerExtensions
    {
        public static async Task<TResult> WithProgressLog<TResult>(this Task<TResult> task, string message)
        {
            Console.Write($"{message}... ");
            var result = await task;
            Console.WriteLine("done.");
            return result;
        }

        public static Task WithProgressLog(this Task task, string message)
        {
            return task.ContinueWith(_ => Task.FromResult(true)).WithProgressLog(message);
        }

        public static TResult WithProgressLog<TResult>(string message, Func<TResult> func)
        {
            return Task.Run(func).WithProgressLog(message).GetAwaiter().GetResult();
        }

        public static void ProgressLog(string message, Action action)
        {
            Task.Run(action).WithProgressLog(message).GetAwaiter().GetResult();
        }
    }
}
