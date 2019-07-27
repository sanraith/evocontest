namespace evorace.Runner.Host.Extensions
{
    public interface IResolvable
    { }

    public interface IResolvable<TResolveAs> where TResolveAs : class
    { }
}
