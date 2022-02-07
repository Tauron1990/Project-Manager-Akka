using JetBrains.Annotations;
using Tauron.Applicarion.Redux.Internal;

namespace Tauron.Applicarion.Redux;

public sealed record ActionState<TState, TAction>(TState State, TAction Action);

public interface IActionDispatcher
{
    IObservable<TAction> ObservAction<TAction>();

    void Dispatch(object action);
}

[PublicAPI]
public interface IReduxStore<TState> : IActionDispatcher, IDisposable
{
    IObservable<TResult> Select<TResult>(Func<TState, TResult> selector);

    IObservable<TState> Select();

    IObservable<ActionState<TState, TAction>> ObservActionState<TAction>();

    IObservable<TResult> ObservAction<TAction, TResult>(Func<TState, TAction, TResult> selector);

    TState CurrentState { get; }

    void Reset();
    
    
    void RegisterMiddlewares(IEnumerable<IMiddleware<TState>> middlewares);

    TMiddleware GetMiddleware<TMiddleware>(Func<TMiddleware> factory)
        where TMiddleware : IMiddleware<TState>;
    
    void RegisterReducers(IEnumerable<On<TState>> reducers);
    
    void RegisterEffects(IEnumerable<Effect<TState>> effects);
    
    void RegisterMiddlewares(params IMiddleware<TState>[] middlewares);
    
    void RegisterReducers(params On<TState>[] reducers);
    
    void RegisterEffects(params Effect<TState>[] effects);
    
    IObservable<object> ObserveAction();
    
    IObservable<TAction> ObserveAction<TAction>()
        where TAction : class;

    IObservable<TResult> ObserveAction<TAction, TResult>(Func<TAction, TState, TResult> resultSelector)
        where TAction : class;
}