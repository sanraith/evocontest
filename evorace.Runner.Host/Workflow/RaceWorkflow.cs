using evorace.Runner.Host.Core;
using System;
using System.Threading.Tasks;

namespace evorace.Runner.Host.Workflow
{
    public class RaceWorkflow : IResolvable
    {
        public Task ExecuteAsync()
        {
            Console.WriteLine("Running race...");
            return Task.CompletedTask;
        }
    }
}
