using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace evocontest.Runner.Host.Core
{
    public class StoppableJobHandler<TJob> : IDisposable
    {
        protected Func<TJob, Task>? JobHandler { get; set; }

        protected Action<Exception>? ExceptionHandler { get; set; }

        public StoppableJobHandler(Func<TJob, Task>? jobHandler = null, Action<Exception>? exceptionHandler = null)
        {
            JobHandler = jobHandler;
            ExceptionHandler = exceptionHandler;
        }

        public void Start()
        {
            myConsumingTask = Task.Run(HandleJobsAsync, CancellationToken.None);
        }

        public void Enqueue(params TJob[] jobs)
        {
            lock (myAddLock)
            {
                if (myJobs.IsCompleted)
                {
                    return;
                }

                foreach (var job in jobs)
                {
                    myJobs.Add(job, CancellationToken.None);
                }
            }
        }

        public async Task StopAsync()
        {
            lock (myAddLock)
            {
                myJobs.CompleteAdding();
            }

            await myConsumingTask;
        }

        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
            myJobs?.Dispose();
        }

        private async Task HandleJobsAsync()
        {
            // ReSharper disable once InconsistentlySynchronizedField
            foreach (var job in myJobs.GetConsumingEnumerable(CancellationToken.None))
            {
                try
                {
                    await (JobHandler?.Invoke(job) ?? Task.CompletedTask);
                }
                catch (Exception e)
                {
                    ExceptionHandler?.Invoke(e);
                }
            }
        }

        private Task myConsumingTask = Task.CompletedTask;
        private readonly object myAddLock = new object();
        private readonly BlockingCollection<TJob> myJobs = new BlockingCollection<TJob>();
    }
}