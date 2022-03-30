using System;
using Stl.Fusion;

namespace SimpleProjectManager.Client.Shared;

public static class StateExtensions
{
    public static IDisposable Subscribe<TValue>(this IState<TValue> state, Action<TValue> action)
        => new Subscriber<TValue>(state, action);
    
    private sealed class Subscriber<TValue> : IDisposable
    {
        private readonly IState<TValue> _state;
        private readonly Action<TValue> _action;

        public Subscriber(IState<TValue> state, Action<TValue> action)
        {
            _state = state;
            _action = action;
            
            _state.AddEventHandler(StateEventKind.All, RunHandler);
        }

        private void RunHandler(IState<TValue> state, StateEventKind _)
        {
            if(state.HasValue)
                _action(state.Value);
        }

        public void Dispose() => _state.RemoveEventHandler(StateEventKind.All, RunHandler);
    }
}