using System;

namespace evorace.Runner.Common.Utility
{
    public static class DisposableValue
    {
        public static DisposableValue<TValue> Create<TValue>(TValue value, Action<TValue> onDisposing)
        {
            return new DisposableValue<TValue>(value, onDisposing);
        }
    }

    public sealed class DisposableValue<TValue> : IDisposable
    {
        public TValue Value { get; }

        public DisposableValue(TValue value, Action<TValue> onDisposing)
        {
            Value = value;
            myOnDispose = onDisposing;
        }

        public void Dispose()
        {
            myOnDispose?.Invoke(Value);
        }

        private readonly Action<TValue> myOnDispose;
    }
}
