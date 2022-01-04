using System.Reactive.Linq;
using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux;

[PublicAPI]
public abstract class Middleware<TState> : IMiddleware<TState>
{
    protected sealed record TypedDispatchedAction<TAction>(TState State, TAction Action);
    
    private List<TypedDispatchedAction<TState>> _processors = new();

    protected IStore<TState> Store { get; private set; } = null!;
    
    public virtual void Initialize(IStore<TState> store)
        => Store = store;

    public IObservable<TState> Connect(IObservable<DispatchedAction<TState>> action, DispatchNext<TState> next)
    {
        Initialize();
        
        return action.SelectMany()
    }

    protected void Dispatch(object action)
        => Store.Dispatch(action);

    protected abstract void Initialize();
}