namespace evocontest.Runner.Host.Core
{
    public interface IResolvable
    { }

    public interface IResolvable<TResolveAs> where TResolveAs : class
    { }
}
