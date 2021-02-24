using System;
using System.Collections.Generic;

namespace Tauron.Application.Workshop.StateManagement.StatePooling
{
    public sealed class StatePool
    {
        private readonly Dictionary<Type, IState> _pooled;

        public IState Get(Type key, object[] parms)
        {
            
        }
    }
}