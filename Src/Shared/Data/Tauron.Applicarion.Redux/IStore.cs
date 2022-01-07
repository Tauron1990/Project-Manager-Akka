using JetBrains.Annotations;
using Tauron.Applicarion.Redux.Internal;

namespace Tauron.Applicarion.Redux;

public sealed record ActionState<TState, TAction>(TState State, TAction Action);

[PublicAPI]
public interface IStore<TState> : IDisposable
{
    IObservable<TResult> Select<TResult>(Func<TState, TResult> selector);

    IObservable<TState> Select();

    IObservable<ActionState<TState, TAction>> ObservAction<TAction>();

    IObservable<TResult> ObservAction<TAction, TResult>(Func<TState, TAction, TResult> selector);

    TState CurrentState { get; }

    void Reset();
    
    void Dispatch(object action);
    
    void RegisterMiddlewares(IEnumerable<IMiddleware<TState>> middlewares);

    TMiddleware Get<TMiddleware>(Func<TMiddleware> factory)
        where TMiddleware : IMiddleware<TState>;
    
    void RegisterReducers(IEnumerable<On<TState>> reducers);
    
    void RegisterEffects(IEnumerable<Effect<TState>> effects);
    
    void RegisterMiddlewares(params IMiddleware<TState>[] middlewares);
    
    void RegisterReducers(params On<TState>[] reducers);
    
    void RegisterEffects(params Effect<TState>[] effects);
}