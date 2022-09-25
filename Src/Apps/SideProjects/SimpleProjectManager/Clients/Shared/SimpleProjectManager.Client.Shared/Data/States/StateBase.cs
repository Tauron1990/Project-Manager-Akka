using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleProjectManager.Client.Shared.Services;
using Stl.Fusion;
using Tauron;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;

namespace SimpleProjectManager.Client.Shared.Data.States;

public abstract class StateBase
{
    protected IStateFactory StateFactory { get; }


    protected StateBase(IStateFactory stateFactory)
        => StateFactory = stateFactory;

    protected IObservable<TValue> FromServer<TValue>(Func<CancellationToken, Task<TValue>> fetcher)
        => Observable.Create<TValue>(
            o =>
            {
                var state = StateFactory.NewComputed<TValue>(async (_, t) => await fetcher(t));

                return new CompositeDisposable(state, state.ToObservable(_ => true).Subscribe(o));
            });
}

public interface IStoreInitializer
{
    void RunConfig(IStoreConfiguration configuration);
}

public abstract class StateBase<TState> : StateBase, IStoreInitializer
    where TState : class, new()
{
    
    protected Action<object> Dispatch { get; private set; } =_ => { };

    protected StateBase(IStateFactory stateFactory)
        : base(stateFactory)
    {

    }


    void IStoreInitializer.RunConfig(IStoreConfiguration configuration)
    {
        configuration.NewState<TState>(
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