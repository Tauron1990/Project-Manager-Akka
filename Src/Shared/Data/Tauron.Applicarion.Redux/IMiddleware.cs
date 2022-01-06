namespace Tauron.Applicarion.Redux;

public delegate IObservable<TState> DispatchNext<TState>(IObservable<DispatchedAction<TState>> action);

public sealed record DispatchedAction<TState>(TState State, object Action);

public interface IMiddleware<TState>
{
    public void Initialize(IStore<TState> store);
    
    public IObservable<TState> Connect(IObservable<DispatchedAction<TState>> actionObservable, DispatchNext<TState> next);
}