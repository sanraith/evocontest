using System;
using System.IO;
using System.Reflection;
using evorace.Runner.Common.Utility;

namespace evorace.Runner.Worker.Core
{
    public sealed class AssemblyLoader
    {
        public static DisposableValue<Assembly> Load(string assemblyPath)
        {
            var context = new CollectibleAssemblyLoadContext();
            var assembly = context.LoadFromAssemblyPath(assemblyPath);

            return DisposableValue.Create(assembly, _ => context.Unload());
        }
    }
}
