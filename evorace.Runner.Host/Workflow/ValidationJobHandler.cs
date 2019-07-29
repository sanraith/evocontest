using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using evorace.Runner.Host.Core;

namespace evorace.Runner.Host.Workflow
{
    public sealed class ValidationJobHandler : IResolvable
    {
        public ValidationJobHandler(Lazy<ValidationWorkflow> validationWorkflow)
        {
            myValidationWorkflow = validationWorkflow;
        }

        public void Start()
        {
            myConsumingTask = Task.Run(OnStart, CancellationToken.None);
        }

        public void Enqueue(params string[] submissionIds)
        {
            lock (myAddLock)
            {
                if (myJobs.IsCompleted)
                {
                    return;
                }

                foreach (var submissionId in submissionIds)
                {
                    myJobs.Add(submissionId, CancellationToken.None);
                }
            }
        }

        public async Task Stop()
        {
            lock (myAddLock)
            {
                myJobs.CompleteAdding();
            }

            await myConsumingTask;
        }

        private async Task OnStart()
        {
            // ReSharper disable once InconsistentlySynchronizedField
            foreach (var submissionId in myJobs.GetConsumingEnumerable(CancellationToken.None))
            {
                await myValidationWorkflow.Value.Execute(submissionId);
            }
        }

        private Task myConsumingTask = Task.CompletedTask;
        private readonly object myAddLock = new object();
        private readonly Lazy<ValidationWorkflow> myValidationWorkflow;
        private readonly BlockingCollection<string> myJobs = new BlockingCollection<string>();
    }
}