using System;
using JetBrains.Annotations;
using Stl;
using Stl.Collections;
using Stl.Conversion;
using Stl.Fusion;

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
        private Func<TArg, TData>? _input;
        private TArg _arg;

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