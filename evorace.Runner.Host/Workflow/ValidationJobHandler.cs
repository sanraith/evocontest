using System;
using System.Threading.Tasks;
using evorace.Runner.Host.Core;

namespace evorace.Runner.Host.Workflow
{
    public sealed class ValidationJobHandler : StoppableJobHandler<string>, IResolvable
    {
        public ValidationJobHandler(Lazy<ValidationWorkflow> validationWorkflow)
        {
            myValidationWorkflowLazy = validationWorkflow;
            JobHandler = HandleJobAsync;
            Start();
        }

        private Task HandleJobAsync(string submissionId)
        {
            return myValidationWorkflowLazy.Value.ExecuteAsync(submissionId);
        }

        private readonly Lazy<ValidationWorkflow> myValidationWorkflowLazy;
    }
}