using System.Reactive.Linq;
using JetBrains.Annotations;
using Tauron.Applicarion.Redux.Extensions.Cache;

namespace Tauron.Applicarion.Redux.Extensions;

public delegate TSelect Selector<in TState, out TSelect>(TState toSelect);

public delegate Task<TData> Reqester<TData>(TData input);

public delegate TState Patcher<in TData, TState>(TData data, TState state);

[PublicAPI]
public class DynamicSource
{
    #region Async

    private static IObservable<TState> CreateAsyncStateObservable<TState>(Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => Observable.FromAsync(source)
           .OnErrorResumeNext(
                e => errorHandler is null
                    ? Observable.Empty<TState>()
                    : Observable.Return(errorHandler(e)).NotNull());

    public static void FromAsync<TState>(IStore<TState> store, Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => store.RegisterEffects(Create.Effect<TState>(() => CreateAsyncStateObservable(source, errorHandler).Select(MutateCallback.Create)));

    public static void FromAsync<TState>(IStore<TState> store, Task<TState> source, Func<Exception, TState?>? errorHandler)
        => FromAsync(store, () => source, errorHandler);

    public static void FromAsync<TState>(IStore<TState> store, Func<Task<TState>> source)
        => FromAsync(store, source, null);

    public static void FromAsync<TState>(IStore<TState> store, Task<TState> source)
        => FromAsync(store, () => source);

    #endregion

    #region Request

    private static Func<IObservable<TState>> CreateRequestBuilder<TState, TSelect>(
        IStore<TState> store,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher,
        Func<Exception, TState?>? errorHandler)
        => () => CreateRequestObservable(store, selector, reqester, patcher, errorHandler);

    private static IObservable<TState> CreateRequestObservable<TState, TSelect>(
        IStore<TState> store,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher,
        Func<Exception, TState?>? errorHandler)
        => Observable.Defer(() => Observable.Start(() => store.CurrentState))
           .Select(state => (state, data: selector(state)))
           .SelectMany(
                async input =>
                {
                    var (state, data) = input;
                    var requestResult = await reqester(data);

                    return (state, requestResult);
                })
           .Select(data => patcher(data.requestResult, data.state))
           .OnErrorResumeNext(e => errorHandler is null ? Observable.Empty<TState>() : Observable.Return(errorHandler(e)).NotNull());

    public static void FromRequest<TState, TSelect>(
        IStore<TState> store,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher,
        Func<Exception, TState?>? errorHandler)
        => store.RegisterEffects(
            Create.Effect<TState>(s => CreateRequestObservable(s, selector, reqester, patcher, errorHandler).Select(MutateCallback.Create)));

    public static void FromRequest<TState, TSelect>(
        IStore<TState> store,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher)
        => FromRequest(store, selector, reqester, patcher, null);
    
    public static void FromRequest<TState>(IStore<TState> store, Reqester<TState> reqester, Func<Exception, TState?>? errorHandler)
        => store.RegisterEffects(
            Create.Effect<TState>(s => CreateRequestObservable(s, state => state, reqester, (state, _) => state, errorHandler).Select(MutateCallback.Create)));

    public static void FromRequest<TState>(IStore<TState> store, Reqester<TState> reqester)
        => FromRequest(store, reqester, null);

    #endregion

    #region Cache

    private static IObservable<TState> CreateCacheObservable<TState>(StateDb stateDb, Func<Exception, TState?>? errorHandler)
        => Observable.Defer(() => Observable.FromAsync(stateDb.Get<TState>))
           .NotNull()
           .OnErrorResumeNext(e => errorHandler == null ? Observable.Empty<TState>() : Observable.Return(errorHandler(e)).NotNull());

    private static IObservable<TState> CreateCacheRequestObservable<TState, TSelect>(
        IStore<TState> store,
        StateDb stateDb,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher,
        Func<Exception, TState?>? errorHandler)
        => CreateCacheObservable(stateDb, errorHandler).Concat(CreateRequestObservable(store, selector, reqester, patcher, errorHandler));

    private static IObservable<TState> CreateCacheAsyncObservable<TState>(StateDb stateDb, Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => CreateCacheObservable(stateDb, errorHandler).Concat(CreateAsyncStateObservable(source, errorHandler));

    public static void FromCache<TState>(IStore<TState> store, StateDb stateDb, Func<Exception, TState?>? errorHandler)
        => store.RegisterEffects(Create.Effect<TState>(() => CreateCacheObservable(stateDb, errorHandler).Select(MutateCallback.Create)));

    public static void FromCache<TState>(IStore<TState> store, StateDb stateDb)
        => FromCache(store, stateDb, null);

    public static void FromCacheAndAsync<TState>(IStore<TState> store, StateDb stateDb, Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => store.RegisterEffects(Create.Effect<TState>(() => CreateCacheAsyncObservable(stateDb, source, errorHandler).Select(MutateCallback.Create)));

    public static void FromCacheAndAsync<TState>(IStore<TState> store, StateDb stateDb, Task<TState> source, Func<Exception, TState?>? errorHandler)
        => FromCacheAndAsync(store, stateDb, () => source, errorHandler);

    public static void FromCacheAndAsync<TState>(IStore<TState> store, StateDb stateDb, Func<Task<TState>> source)
        => FromCacheAndAsync(store, stateDb, source, null);

    public static void FromCacheAndAsync<TState>(IStore<TState> store, StateDb stateDb, Task<TState> source)
        => FromCacheAndAsync(store, stateDb, () => source, null);

    public static void FromCacheAndRequest<TState, TSelect>(
        IStore<TState> store,
        StateDb stateDb,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher,
        Func<Exception, TState?>? errorHandler)
        => store.RegisterEffects(
            Create.Effect<TState>(
                () => CreateCacheRequestObservable(store, stateDb, selector, reqester, patcher, errorHandler)
                   .Select(MutateCallback.Create)));

    public static void FromCacheAndRequest<TState, TSelect>(
        IStore<TState> store,
        StateDb stateDb,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher)
        => FromCacheAndRequest(store, stateDb, selector, reqester, patcher, null);

    public static void FromCacheAndRequest<TState>(IStore<TState> store, StateDb stateDb, Reqester<TState> reqester, Func<Exception, TState?>? errorHandler)
        => FromCacheAndRequest(store, stateDb, s => s, reqester, (s, _) => s, errorHandler);

    public static void FromCacheAndRequest<TState>(IStore<TState> store, StateDb stateDb, Reqester<TState> reqester)
        => FromCacheAndRequest(store, stateDb, reqester, null);

    #endregion
}