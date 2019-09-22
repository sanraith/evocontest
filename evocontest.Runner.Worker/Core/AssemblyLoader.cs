using System;
using System.IO;
using System.Reflection;
using evocontest.Runner.Common.Utility;

namespace evocontest.Runner.Worker.Core
{
    public sealed class AssemblyLoader
    {
        public static DisposableValue<Assembly> Load(string assemblyPath)
        {
            var context = new CollectibleAssemblyLoadContext();
            var assemblyFile = new FileInfo(assemblyPath);
            var assembly = context.LoadFromAssemblyPath(assemblyFile.FullName);

            return DisposableValue.Create(assembly, _ => context.Unload());
        }
    }
}
