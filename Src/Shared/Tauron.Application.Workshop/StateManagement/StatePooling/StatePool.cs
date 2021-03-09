using System;
using System.Collections.Generic;
using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement.StatePooling
{
    public sealed class StatePool
    {
        private readonly Dictionary<Type, IStateInstance> _pooled = new();

        public IStateInstance? Get(Type key, Func<IStateInstance?> factory)
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