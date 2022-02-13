﻿using System.Reactive.Linq;

namespace Tauron.Applicarion.Redux.Extensions.Internal;

public static class AsyncRequestMiddleware
{
    public static AsyncRequestMiddleware<TState> Get<TState>(IReduxStore<TState> store)
        => store.GetMiddleware(AsyncRequestMiddleware<TState>.Factory);
}

public sealed class AsyncRequestMiddleware<TState> : Middleware<TState>
{
    internal static readonly Func<AsyncRequestMiddleware<TState>> Factory = () => new AsyncRequestMiddleware<TState>();
    
    private TState GetState() => Store.CurrentState;
    
    public void AddRequest<TAction>(
        Func<TAction, Task<string?>> runRequest,
        Func<TState, TAction, TState> onScess,
        Func<TState, object, TState> onFail)
        where TAction : class
    {
        IObservable<DispatchedAction<TState>> Runner(IObservable<TypedDispatechedAction<TAction>> arg)
            => arg.SelectManySafe(async input => (input.Action, result:await runRequest(input.Action)))
               .ConvertResult(
                    pair => string.IsNullOrWhiteSpace(pair.result) ? onScess(GetState(), pair.Action) : onFail(GetState(), pair.result), 
                    exception => onFail(GetState(), exception))
               .Select(s => new DispatchedAction<TState>(s, MutateCallback.Create(s)));
        
        OnAction<TAction>(Runner);
    }
    
    public void AddRequest<TAction>(
        Func<TAction, ValueTask<string?>> runRequest,
        Func<TState, TAction, TState> onScess,
        Func<TState, object, TState> onFail)
        where TAction : class
    {
        IObservable<DispatchedAction<TState>> Runner(IObservable<TypedDispatechedAction<TAction>> arg)
            => arg.SelectManySafe(async input => (input.Action, result:await runRequest(input.Action)))
               .ConvertResult(
                    pair => string.IsNullOrWhiteSpace(pair.result) ? onScess(GetState(), pair.Action) : onFail(GetState(), pair.result), 
                    exception => onFail(GetState(), exception))
               .Select(s => new DispatchedAction<TState>(s, MutateCallback.Create(s)));
        
        OnAction<TAction>(Runner);
    }
}