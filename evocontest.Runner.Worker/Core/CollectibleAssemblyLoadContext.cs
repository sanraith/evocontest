using System.Runtime.Loader;

namespace evocontest.Runner.Worker.Core
{
    public class CollectibleAssemblyLoadContext : AssemblyLoadContext
    {
        public CollectibleAssemblyLoadContext() : base(isCollectible: true)
        { }
    }
}
