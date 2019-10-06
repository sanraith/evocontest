using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace evocontest.Runner.Host.Common.Utility
{
    public static class TaskHelper
    {
        /// <summary>
        /// .
        /// </summary>
        /// <exception cref="TimeoutException"></exception>
        public static Task TimedTask(int timeoutMillis, Func<Task> taskFunc)
        {
            return TimedTask(TimeSpan.FromMilliseconds(timeoutMillis), taskFunc);
        }

        /// <summary>
        /// .
        /// </summary>
        /// <exception cref="TimeoutException"></exception>
        public static Task TimedTask(TimeSpan timeout, Func<Task> taskFunc)
        {
            return TimedTask(timeout, () => taskFunc().ContinueWith(_ => true));
        }

        /// <summary>
        /// .
        /// </summary>
        /// <exception cref="TimeoutException"></exception>
        public static Task<TResult> TimedTask<TResult>(int timeoutMillis, Func<Task<TResult>> workTaskGenerator)
        {
            return TimedTask(TimeSpan.FromMilliseconds(timeoutMillis), workTaskGenerator);
        }

        public static async Task<TResult> TimedTask<TResult>(TimeSpan timeout, Func<Task<TResult>> workTaskGenerator)
        {
            Exception disqualifyException = null;
            Task<TResult> workTask;
            Task timerTask;

            try
            {
                timerTask = Task.Delay(timeout);
                workTask = workTaskGenerator();
                var completedTask = await Task.WhenAny(timerTask, workTask);
                var success = workTask.Equals(completedTask);
                if (success)
                {
                    return workTask.Result;
                }
            }
            catch (Exception ex)
            {
                disqualifyException = ex;
            }
            finally
            {
                // TODO cancel task?
            }

            throw disqualifyException ?? new TimeoutException();
        }
    }
}
