using System.Reactive.Linq;
using JetBrains.Annotations;
using Stl.Fusion;
using Tauron.Applicarion.Redux.Extensions.Internal;

namespace Tauron.Applicarion.Redux.Extensions;

[PublicAPI]
public static class DynamicUpdate
{
    private static Effect<TState> CreateDynamicUpdaterInternal<TState, TSource, TData>(
        IStateFactory stateFactory,
        Selector<TState, TSource> selector, 
        Func<CancellationToken, Func<CancellationToken, ValueTask<TSource>>, Task<TData>> requester, 
        Patcher<TData, TState> patcher, 
        Action<Exception>? errorHandler)
        => Create.Effect<TState>(
            store => Observable.Create<object>(o =>
                                               {
                                                   var stlState = stateFactory.NewMutable<TSource>();
                                                   var stateUpdateTrigger = store.Select(state => selector(state)).Subscribe(source => stlState.Set(source));
                                                   
                                                   var computer = stateFactory.NewComputed(
                                                       new ComputedState<TData>.Options(),
                                                       async (_, token) => await requester(token, stlState.Use).ConfigureAwait(false));

                                                   var observableSubscription = computer
                                                      .ToObservable(
                                                           ex =>
                                                           {
                                                               errorHandler?.Invoke(ex);
                                                               return false;
                                                           })
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
        Patcher<TData, TState> patcher,
        Action<Exception>? errorHandler)
        => store.RegisterEffects(CreateDynamicUpdaterInternal(stateFactory, sourceSelector, fetcher, patcher, errorHandler));

    public static void OnTheFlyUpdate<TState, TData>(
        IReduxStore<TState> store,
        IStateFactory stateFactory,
        Func<CancellationToken, Task<TData>> fetcher,
        Patcher<TData, TState> patcher,
        Action<Exception>? errorHandler)
        => store.RegisterEffects(
            CreateDynamicUpdaterInternal(
                stateFactory,
                _ => 0,
                async (token, sel) =>
                {
                    await sel(token).ConfigureAwait(false);

                    return await fetcher(token).ConfigureAwait(false);
                },
                patcher, errorHandler));
}