namespace Tauron.Applicarion.Redux;

public sealed record ActionState<TState, TAction>(TState State, TAction Action);

public interface IStore<TState>
{
    IObservable<TResult> Select<TResult>(Func<TState, TResult> selector);

    IObservable<TState> Select();

    IObservable<ActionState<TState, TAction>> ObservAction<TAction>();

    IObservable<TResult> ObservAction<TAction, TResult>(Func<TState, TAction, TResult> selector);

    TState CurrentState { get; }
    
    void Dispatch(object action);
    
    void RegisterMiddlewares(IEnumerable<IMiddleware<TState>> middlewares);
    
    void RegisterReducers(IEnumerable<On<TState>> reducers);
    
    void RegisterEffects(IEnumerable<Effect<TState>> effects);
}