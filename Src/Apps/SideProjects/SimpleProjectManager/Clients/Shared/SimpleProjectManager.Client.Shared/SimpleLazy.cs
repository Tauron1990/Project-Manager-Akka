using System;
using JetBrains.Annotations;
using Vogen;

namespace SimpleProjectManager.Client.Shared;

public static class SimpleLazy
{
    public static Lazy<TData> Create<TData, TArg>(TArg arg, Func<TArg, TData> factory)
        => new StateFullLazy<TData, TArg>(factory, arg);

    [PublicAPI]
    public abstract class Lazy<TData>
    {
        private TData? _value;

        public TData Value => _value ??= Create();

        protected abstract TData Create();

        private TData GetValue()
            => Value;

        public static implicit operator Func<TData>(Lazy<TData> lazy)
            => lazy.GetValue;
    }

    private sealed class StateFullLazy<TData, TArg> : Lazy<TData>
    {
        private TArg _arg;
        private Func<TArg, TData>? _input;

        internal StateFullLazy(Func<TArg, TData> input, TArg arg)
        {
            _input = input;
            _arg = arg;
        }

        protected override TData Create()
        {
            if(_input is null)
                throw new InvalidOperationException("No Input Func Found or Create Called Tweice");

            TData result = _input(_arg);
            _input = null;
            _arg = default!;

            return result;
        }
    }
}