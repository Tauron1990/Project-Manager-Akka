using System.Reactive.Disposables;
using System.Reactive.Linq;
using Stl.Fusion;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Data.States;

public abstract class StateBase
{
    private IStateFactory StateFactory { get; }
    

    protected StateBase(IStateFactory stateFactory)
        => StateFactory = stateFactory;
    
    protected IObservable<TValue> FromServer<TValue>(Func<CancellationToken, Task<TValue>> fetcher)
        => Observable.Create<TValue>(
            o =>
            {
                var state = StateFactory.NewComputed<TValue>(async (_, t) => await fetcher(t));

                return new CompositeDisposable(state, state.ToObservable(true).Subscribe(o));
            });
}

public abstract class StateBase<TState> : StateBase
    where TState : class, new()
{
    
    protected Action<object> Dispatch { get; private set; } =_ => { };

    protected StateBase(IStoreConfiguration storeConfiguration, IStateFactory stateFactory)
        : base(stateFactory)
    {
        storeConfiguration.NewState<TState>(
            s => ConfigurateState(s).AndFinish(
                store =>
                {
                    Dispatch = store.Dispatch;
                    PostConfiguration(store.ForState<TState>());
                }));
    }

    protected abstract IStateConfiguration<TState> ConfigurateState(ISourceConfiguration<TState> configuration);

    protected virtual void PostConfiguration(IRootStoreState<TState> state)
    {
        
    }
}