using System;

namespace evocontest.Runner.Common.Utility
{
    public static class DisposableValue
    {
        public static DisposableValue<TValue> Create<TValue>(TValue value, params IDisposable[] disposables)
        {
            return new DisposableValue<TValue>(value, onDispose: null, disposables);
        }

        public static DisposableValue<TValue> Create<TValue>(TValue value, Action<TValue> onDispose, params IDisposable[] disposables)
        {
            return new DisposableValue<TValue>(value, onDispose, disposables);
        }

    }

    public sealed class DisposableValue<TValue> : IDisposable
    {
        public TValue Value { get; }

        public DisposableValue(TValue value, Action<TValue>? onDispose, params IDisposable[] disposables)
        {
            Value = value;
            myOnDispose = onDispose;
            myDisposables = disposables;
        }

        public void Dispose()
        {
            myOnDispose?.Invoke(Value);
            foreach (var disposable in myDisposables)
            {
                disposable.Dispose();
            }
        }

        private readonly Action<TValue>? myOnDispose;
        private readonly IDisposable[] myDisposables;
    }
}
