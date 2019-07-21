using System.Runtime.Loader;

namespace evorace.Runner.Worker.Core
{
    public class CollectibleAssemblyLoadContext : AssemblyLoadContext
    {
        public CollectibleAssemblyLoadContext() : base(isCollectible: true)
        { }
    }
}
