using evorace.Runner.Host.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace evorace.Runner.Host.Workflow
{
    public class RaceWorkflow : IResolvable
    {
        public Task Execute()
        {
            Console.WriteLine("Running race...");
            return Task.CompletedTask;
        }
    }
}
