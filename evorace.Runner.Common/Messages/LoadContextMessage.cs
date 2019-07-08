using System;

namespace evorace.Runner.Common.Messages
{
    /// <summary>
    /// Instructs the receiver to load the target assemblly.
    /// </summary>
    [Serializable]
    public sealed class LoadContextMessage : AbstractMessage
    {
        public string TargetAssemblyPath { get; set; }

        private LoadContextMessage()
        {
            TargetAssemblyPath = null!;
        }

        public LoadContextMessage(string contesterAssemblyName)
        {
            TargetAssemblyPath = contesterAssemblyName;
        }
    }
}
