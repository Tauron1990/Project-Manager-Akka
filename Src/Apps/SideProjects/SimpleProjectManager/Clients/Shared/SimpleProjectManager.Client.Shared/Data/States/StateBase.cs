using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Stl;
using Stl.Fusion;
using Tauron;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;

namespace SimpleProjectManager.Client.Shared.Data.States;

public abstract class StateBase
{
    protected StateBase(IStateFactory stateFactory)
        => StateFactory = stateFactory;

    protected IStateFactory StateFactory { get; }

    protected IObservable<TValue> FromServer<TValue>(Func<CancellationToken, Task<TValue>> fetcher, TValue defaultValue)
        => Observable.Create<TValue>(
            o =>
            {
                var state = StateFactory.NewComputed(Result.Value(defaultValue), async (_, t) => await fetcher(t).ConfigureAwait(false));

                return new CompositeDisposable(state, state.ToObservable(_ => true).Subscribe(o));
            });
}

public abstract class StateBase<TState> : StateBase, IStoreInitializer
    where TState : class, new()
{
    protected StateBase(IStateFactory stateFactory)
        : base(stateFactory) { }

    protected Action<object> Dispatch { get; private set; } = _ => { };


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

    protected virtual void PostConfiguration(IRootStoreState<TState> state) { }
}