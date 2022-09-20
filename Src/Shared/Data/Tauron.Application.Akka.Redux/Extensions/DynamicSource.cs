using System.Reactive;
using Akka;
using Akka.Streams.Dsl;
using Akka.Util;
using JetBrains.Annotations;
using Stl.Fusion;
using Tauron.Application.Akka.Redux.Extensions.Cache;

namespace Tauron.Application.Akka.Redux.Extensions;

public delegate TSelect Selector<in TState, out TSelect>(TState toSelect);

public delegate Task<TData> Reqester<TData>(CancellationToken token, TData input);

public delegate TState Patcher<in TData, TState>(TData data, TState state);

[PublicAPI]
public static class DynamicSource
{
    #region Async

    private static Source<TState, NotUsed> CreateAsyncStateSource<TState>(Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => Source.Single(Unit.Default)
           .SelectAsync(1, _ => source())
           .Recover(ex => 
                    (
                        errorHandler is null
                        ? Option<TState?>.None 
                        : new Option<TState?>(errorHandler(ex))
                        )!)
           .Where(state => state is not null);

    public static void FromAsync<TState>(IReduxStore<TState> store, Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => store.RegisterEffects(Create.Effect<TState>(() => CreateAsyncStateSource(source, errorHandler)
                                                          .Select(s => (object?)MutateCallback.Create(s))));

    public static void FromAsync<TState>(IReduxStore<TState> store, Task<TState> source, Func<Exception, TState?>? errorHandler)
        => FromAsync(store, () => source, errorHandler);

    public static void FromAsync<TState>(IReduxStore<TState> store, Func<Task<TState>> source)
        => FromAsync(store, source, null);

    public static void FromAsync<TState>(IReduxStore<TState> store, Task<TState> source)
        => FromAsync(store, () => source);

    #endregion

    #region Request

    private static Func<Source<TState, NotUsed>> CreateRequestBuilder<TState, TSelect>(
        IStateFactory stateFactory,
        IReduxStore<TState> store,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher,
        Func<Exception, TState?>? errorHandler)
    where TState : class
        => () => CreateRequestSource(stateFactory, store, selector, reqester, patcher, errorHandler);

    private static Source<TState, NotUsed> CreateRequestSource<TState, TSelect>(
        IStateFactory stateFactory,
        IReduxStore<TState> store,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher,
        Func<Exception, TState?>? errorHandler)
        where TState : class
    {
        async Task<TState?> RunRequest(IComputedState<TState?> computedState, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                var currentState = store.CurrentState;
                var data = selector(currentState);
                token.ThrowIfCancellationRequested();
                var requestResult = await reqester(token, data).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                var patch = patcher(requestResult, currentState);
                token.ThrowIfCancellationRequested();
                
                return patch;
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                return errorHandler?.Invoke(e) ?? default(TState);
            }
        }

        var state = stateFactory.NewComputed<TState?>(RunRequest);

        return DynamicUpdate.ToSource(state, true).Where(s => s is not null)!;
    }

    public static void FromRequest<TState, TSelect>(
        IStateFactory stateFactory,
        IReduxStore<TState> store,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher,
        Func<Exception, TState?>? errorHandler)
        where TState : class
        => store.RegisterEffects(
            Create.Effect<TState>(s => CreateRequestSource(stateFactory, s, selector, reqester, patcher, errorHandler)
                                     .Select(state => (object?)MutateCallback.Create(state))));

    public static void FromRequest<TState, TSelect>(
        IStateFactory stateFactory,
        IReduxStore<TState> store,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher)
        where TState : class
        => FromRequest(stateFactory, store, selector, reqester, patcher, null);
    
    public static void FromRequest<TState>(IStateFactory stateFactory, IReduxStore<TState> store, Reqester<TState> reqester, Func<Exception, TState?>? errorHandler)
        where TState : class
        => store.RegisterEffects(
            Create.Effect<TState>(s => CreateRequestSource(stateFactory, s, state => state, reqester, (state, _) => state, errorHandler)
                                     .Select(state => (object?)MutateCallback.Create(state))));

    public static void FromRequest<TState>(IStateFactory stateFactory, IReduxStore<TState> store, Reqester<TState> reqester)
        where TState : class
        => FromRequest(stateFactory, store, reqester, null);

    #endregion

    #region Cache

    private static Source<TState, NotUsed> CreateCacheSource<TState>(StateDb stateDb, Func<Exception, TState?>? errorHandler)
        => Source.Single(Unit.Default)
           .SelectAsync(1, _ => stateDb.Get<TState>())
           .Recover(
                ex => errorHandler is null
                    ? default 
                    : new Option<TState?>(errorHandler(ex)))
           .Where(s => s is not null)!;

    private static Source<TState, NotUsed> CreateCacheRequestSource<TState, TSelect>(
        IStateFactory stateFactory,
        IReduxStore<TState> store,
        StateDb stateDb,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher,
        Func<Exception, TState?>? errorHandler)
        where TState : class
        => CreateCacheSource(stateDb, errorHandler).Concat(CreateRequestSource(stateFactory, store, selector, reqester, patcher, errorHandler));

    private static Source<TState, NotUsed> CreateCacheAsyncObservable<TState>(StateDb stateDb, Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => CreateCacheSource(stateDb, errorHandler).Concat(CreateAsyncStateSource(source, errorHandler));

    public static void FromCache<TState>(IReduxStore<TState> store, StateDb stateDb, Func<Exception, TState?>? errorHandler)
        => store.RegisterEffects(Create.Effect<TState>(() => CreateCacheSource(stateDb, errorHandler)
                                                          .Select(state => (object?)MutateCallback.Create(state))));

    public static void FromCache<TState>(IReduxStore<TState> store, StateDb stateDb)
        => FromCache(store, stateDb, null);

    public static void FromCacheAndAsync<TState>(IReduxStore<TState> store, StateDb stateDb, Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => store.RegisterEffects(Create.Effect<TState>(() => CreateCacheAsyncObservable(stateDb, source, errorHandler)
                                                          .Select(state => (object?)MutateCallback.Create(state))));

    public static void FromCacheAndAsync<TState>(IReduxStore<TState> store, StateDb stateDb, Task<TState> source, Func<Exception, TState?>? errorHandler)
        => FromCacheAndAsync(store, stateDb, () => source, errorHandler);

    public static void FromCacheAndAsync<TState>(IReduxStore<TState> store, StateDb stateDb, Func<Task<TState>> source)
        => FromCacheAndAsync(store, stateDb, source, null);

    public static void FromCacheAndAsync<TState>(IReduxStore<TState> store, StateDb stateDb, Task<TState> source)
        => FromCacheAndAsync(store, stateDb, () => source, null);

    public static void FromCacheAndRequest<TState, TSelect>(
        IStateFactory stateFactory,
        IReduxStore<TState> store,
        StateDb stateDb,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher,
        Func<Exception, TState?>? errorHandler)
        where TState : class
        => store.RegisterEffects(
            Create.Effect<TState>(
                () => CreateCacheRequestSource(stateFactory, store, stateDb, selector, reqester, patcher, errorHandler)
                   .Select(state => (object?)MutateCallback.Create(state))));

    public static void FromCacheAndRequest<TState, TSelect>(
        IStateFactory stateFactory,
        IReduxStore<TState> store,
        StateDb stateDb,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher)
        where TState : class
        => FromCacheAndRequest(stateFactory, store, stateDb, selector, reqester, patcher, null);

    public static void FromCacheAndRequest<TState>(IStateFactory stateFactory, IReduxStore<TState> store, StateDb stateDb, Reqester<TState> reqester, Func<Exception, TState?>? errorHandler)
        where TState : class
        => FromCacheAndRequest(stateFactory, store, stateDb, s => s, reqester, (s, _) => s, errorHandler);

    public static void FromCacheAndRequest<TState>(IStateFactory stateFactory, IReduxStore<TState> store, StateDb stateDb, Reqester<TState> reqester)
        where TState : class
        => FromCacheAndRequest(stateFactory, store, stateDb, reqester, null);

    #endregion
}