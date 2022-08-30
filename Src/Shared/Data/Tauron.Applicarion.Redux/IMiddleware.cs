namespace Tauron.Applicarion.Redux;

public delegate IObservable<DispatchedAction<TState>> DispatchNext<TState>(IObservable<DispatchedAction<TState>> action);

public sealed record DispatchedAction<TState>(TState State, object? Action);

public interface IMiddleware<TState>
{
    public void Initialize(IReduxStore<TState> store);
    
    public IObservable<DispatchedAction<TState>> Connect(IObservable<DispatchedAction<TState>> actionObservable);
}
