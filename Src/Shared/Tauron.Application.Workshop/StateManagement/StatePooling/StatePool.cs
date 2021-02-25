using System;
using System.Collections.Generic;

namespace Tauron.Application.Workshop.StateManagement.StatePooling
{
    public sealed class StatePool
    {
        private readonly Dictionary<Type, IState> _pooled = new();

        public IState? Get(Type key, Func<IState?> factory)
        {
            if (_pooled.TryGetValue(key, out var inst))
                return inst;

            inst = factory();
            if (inst == null)
                return null;
            _pooled.Add(key, inst);
            return inst;
        }
    }
}