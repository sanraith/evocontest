using System;

namespace evocontest.Runner.Host.Common.Utility
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

    /// <summary>
    /// Represents a value with attached disposable instances and logic.
    /// When this instance is disposed:<para />
    ///     1. The onDispose action is executed.<para />
    ///     2. The items of the disposables array are disposed in order.
    /// </summary>
    /// <typeparam name="TValue">The type of the contained value.</typeparam>
    public sealed class DisposableValue<TValue> : IDisposable
    {
        public TValue Value { get; }

        internal DisposableValue(TValue value, Action<TValue> onDispose, params IDisposable[] disposables)
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

        private readonly Action<TValue> myOnDispose;
        private readonly IDisposable[] myDisposables;
    }
}
