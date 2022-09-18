using System.Reactive.Linq;
using JetBrains.Annotations;
using Stl.Fusion;
using Tauron.Application.Akka.Redux.Extensions.Internal;

namespace Tauron.Application.Akka.Redux.Extensions;

[PublicAPI]
public static class DynamicUpdate
{
    internal static IObservable<TData> ToObservable<TData>(IState<TData> state, bool skipErrors = false)
        => Observable.Create<TData>(o =>
                                    {
                                        if(state.HasValue)
                                            o.OnNext(state.Value);
                                        return new StateRegistration<TData>(o, state, skipErrors);
                                    })
           .DistinctUntilChanged()
           .Replay(1).RefCount();
        
    private sealed class StateRegistration<TData> : IDisposable
    {
        private readonly IObserver<TData> _observer;
        private readonly IState<TData> _state;
        private readonly bool _skipErrors;

        internal StateRegistration(IObserver<TData> observer, IState<TData> state, bool skipErrors)
        {
            _observer = observer;
            _state = state;
            _skipErrors = skipErrors;

            
            
            state.AddEventHandler(StateEventKind.All, Handler);
        }

        private void Handler(IState<TData> arg1, StateEventKind arg2)
        {
            if(_state.HasValue)
                _observer.OnNext(_state.Value);
            else if(!_skipErrors && _state.HasError && _state.Error is not null)
                _observer.OnError(_state.Error);
        }

        public void Dispose() => _state.RemoveEventHandler(StateEventKind.All, Handler);
    }
    
    private static Effect<TState> CreateDynamicUpdaterInternal<TState, TSource, TData>(
        IStateFactory stateFactory,
        Selector<TState, TSource> selector, 
        Func<CancellationToken, Func<CancellationToken, ValueTask<TSource>>, Task<TData>> requester, 
        Patcher<TData, TState> patcher)
        => Create.Effect<TState>(
            store => Observable.Create<object>(o =>
                                               {
                                                   var stlState = stateFactory.NewMutable<TSource>();
                                                   var stateUpdateTrigger = store.Select(state => selector(state)).Subscribe(source => stlState.Set(source));
                                                   
                                                   var computer = stateFactory.NewComputed(
                                                       new ComputedState<TData>.Options(),
                                                       async (_, token) => await requester(token, stlState.Use).ConfigureAwait(false));

                                                   var observableSubscription = ToObservable(computer, true)
                                                      .Select(data => patcher(data, store.CurrentState))
                                                      .Select(MutateCallback.Create)
                                                      .Cast<object>()
                                                      .Subscribe(o);

                                                   return () =>
                                                          {
                                                              computer.Dispose();
                                                              
                                                              stateUpdateTrigger.Dispose();
                                                              observableSubscription.Dispose();
                                                          };
                                               }));

    public static void AddRequest<TState, TAction>(
        IRootStore store,
        Func<TAction, ValueTask<string?>> runRequest, Func<TState, TAction, TState> onScess)
        where TAction : class where TState : new()
        => AsyncRequestMiddleware
           .Get<TState>(store)
           .AddRequest(runRequest, onScess, (state, _) => state);

    public static void AddRequest<TState, TAction>(
        IRootStore store,
        Func<TAction, ValueTask<string?>> runRequest,
        Func<TState, TAction, TState> onScess,
        Func<TState, object, TState> onFail)
        where TAction : class where TState : new()
        => AsyncRequestMiddleware
           .Get<TState>(store)
           .AddRequest(runRequest, onScess, onFail);

    public static void AddRequest<TState, TAction>(IRootStore store, Func<TAction, Task<string?>> runRequest, Func<TState, TAction, TState> onScess)
        where TAction : class where TState : new()
        => AsyncRequestMiddleware
           .Get<TState>(store)
           .AddRequest(runRequest, onScess, (state, _) => state);

    public static void AddRequest<TState, TAction>(
        IRootStore store,
        Func<TAction, Task<string?>> runRequest,
        Func<TState, TAction, TState> onScess,
        Func<TState, object, TState> onFail)
        where TAction : class where TState : new()
        => AsyncRequestMiddleware
           .Get<TState>(store)
           .AddRequest(runRequest, onScess, onFail);

    public static void OnTheFlyUpdate<TState, TSource, TData>(
        IReduxStore<TState> store,
        IStateFactory stateFactory,
        Selector<TState, TSource> sourceSelector,
        Func<CancellationToken, Func<CancellationToken, ValueTask<TSource>>, Task<TData>> fetcher,
        Patcher<TData, TState> patcher)
        => store.RegisterEffects(CreateDynamicUpdaterInternal(stateFactory, sourceSelector, fetcher, patcher));

    public static void OnTheFlyUpdate<TState, TData>(
        IReduxStore<TState> store,
        IStateFactory stateFactory,
        Func<CancellationToken, Task<TData>> fetcher,
        Patcher<TData, TState> patcher)
        => store.RegisterEffects(
            CreateDynamicUpdaterInternal(
                stateFactory,
                _ => 0,
                async (token, sel) =>
                {
                    await sel(token).ConfigureAwait(false);

                    return await fetcher(token).ConfigureAwait(false);
                },
                patcher));
}