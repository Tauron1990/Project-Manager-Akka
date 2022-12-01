using System.Reactive.Linq;
using JetBrains.Annotations;
using Stl.Fusion;
using Tauron.Applicarion.Redux.Extensions.Cache;

namespace Tauron.Applicarion.Redux.Extensions;

[PublicAPI]
public static class DynamicSource
{
    #region Async

    private static IObservable<TState> CreateAsyncStateObservable<TState>(Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => Observable.FromAsync(source)
           .OnErrorResumeNext(
                e => errorHandler is null
                    ? Observable.Empty<TState>()
                    : Observable.Return(errorHandler(e)).NotNull());

    public static void FromAsync<TState>(IReduxStore<TState> store, Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => store.RegisterEffects(Create.Effect<TState>(() => CreateAsyncStateObservable(source, errorHandler).Select(MutateCallback.Create)));

    public static void FromAsync<TState>(IReduxStore<TState> store, Task<TState> source, Func<Exception, TState?>? errorHandler)
        => FromAsync(store, () => source, errorHandler);

    public static void FromAsync<TState>(IReduxStore<TState> store, Func<Task<TState>> source)
        => FromAsync(store, source, errorHandler: null);

    public static void FromAsync<TState>(IReduxStore<TState> store, Task<TState> source)
        => FromAsync(store, () => source);

    #endregion

    #region Request

    private static Func<IObservable<TState>> CreateRequestBuilder<TState, TSelect>(
        IStateFactory stateFactory,
        IReduxStore<TState> store,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher,
        Func<Exception, TState?>? errorHandler)
        where TState : class
        => () => CreateRequestObservable(stateFactory, store, selector, reqester, patcher, errorHandler);

    private static IObservable<TState> CreateRequestObservable<TState, TSelect>(
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
            #if DEBUG
            Console.WriteLine($"Run Request {nameof(TState)}");
            #endif
            
            try
            {
                token.ThrowIfCancellationRequested();
                TState currentState = store.CurrentState;
                TSelect data = selector(currentState);
                token.ThrowIfCancellationRequested();
                TSelect requestResult = await reqester(token, data).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                TState patch = patcher(requestResult, currentState);
                token.ThrowIfCancellationRequested();

                return patch;
            }
            catch (Exception e)
            {
                if(e is OperationCanceledException)
                    return default;
                
                #if DEBUG
                Console.WriteLine($"Error on Process Request; {nameof(TState)}");
                #endif
                return errorHandler?.Invoke(e) ?? default(TState);
            }
        }

        var state = stateFactory.NewComputed<TState?>(RunRequest);

        return state.ToObservable(
                ex =>
                {
                    errorHandler?.Invoke(ex);

                    return false;
                })
           .NotNull();
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
            Create.Effect<TState>(s => CreateRequestObservable(stateFactory, s, selector, reqester, patcher, errorHandler).Select(MutateCallback.Create)));

    public static void FromRequest<TState, TSelect>(
        IStateFactory stateFactory,
        IReduxStore<TState> store,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher)
        where TState : class
        => FromRequest(stateFactory, store, selector, reqester, patcher, errorHandler: null);

    public static void FromRequest<TState>(IStateFactory stateFactory, IReduxStore<TState> store, Reqester<TState> reqester, Func<Exception, TState?>? errorHandler)
        where TState : class
        => store.RegisterEffects(
            Create.Effect<TState>(s => CreateRequestObservable(stateFactory, s, state => state, reqester, (state, _) => state, errorHandler).Select(MutateCallback.Create)));

    public static void FromRequest<TState>(IStateFactory stateFactory, IReduxStore<TState> store, Reqester<TState> reqester)
        where TState : class
        => FromRequest(stateFactory, store, reqester, errorHandler: null);

    #endregion

    #region Cache

    private static IObservable<TState> CreateCacheObservable<TState>(StateDb stateDb, Func<Exception, TState?>? errorHandler)
        => Observable.Defer(() => Observable.FromAsync(stateDb.Get<TState>))
           .NotNull()
           .OnErrorResumeNext(e => errorHandler == null ? Observable.Empty<TState>() : Observable.Return(errorHandler(e)).NotNull());

    private static IObservable<TState> CreateCacheRequestObservable<TState, TSelect>(
        IStateFactory stateFactory,
        IReduxStore<TState> store,
        StateDb stateDb,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher,
        Func<Exception, TState?>? errorHandler)
        where TState : class
        => CreateCacheObservable(stateDb, errorHandler).Concat(CreateRequestObservable(stateFactory, store, selector, reqester, patcher, errorHandler));

    private static IObservable<TState> CreateCacheAsyncObservable<TState>(StateDb stateDb, Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => CreateCacheObservable(stateDb, errorHandler).Concat(CreateAsyncStateObservable(source, errorHandler));

    public static void FromCache<TState>(IReduxStore<TState> store, StateDb stateDb, Func<Exception, TState?>? errorHandler)
        => store.RegisterEffects(Create.Effect<TState>(() => CreateCacheObservable(stateDb, errorHandler).Select(MutateCallback.Create)));

    public static void FromCache<TState>(IReduxStore<TState> store, StateDb stateDb)
        => FromCache(store, stateDb, errorHandler: null);

    public static void FromCacheAndAsync<TState>(IReduxStore<TState> store, StateDb stateDb, Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => store.RegisterEffects(Create.Effect<TState>(() => CreateCacheAsyncObservable(stateDb, source, errorHandler).Select(MutateCallback.Create)));

    public static void FromCacheAndAsync<TState>(IReduxStore<TState> store, StateDb stateDb, Task<TState> source, Func<Exception, TState?>? errorHandler)
        => FromCacheAndAsync(store, stateDb, () => source, errorHandler);

    public static void FromCacheAndAsync<TState>(IReduxStore<TState> store, StateDb stateDb, Func<Task<TState>> source)
        => FromCacheAndAsync(store, stateDb, source, errorHandler: null);

    public static void FromCacheAndAsync<TState>(IReduxStore<TState> store, StateDb stateDb, Task<TState> source)
        => FromCacheAndAsync(store, stateDb, () => source, errorHandler: null);

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
                () => CreateCacheRequestObservable(stateFactory, store, stateDb, selector, reqester, patcher, errorHandler)
                   .Select(MutateCallback.Create)));

    public static void FromCacheAndRequest<TState, TSelect>(
        IStateFactory stateFactory,
        IReduxStore<TState> store,
        StateDb stateDb,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher)
        where TState : class
        => FromCacheAndRequest(stateFactory, store, stateDb, selector, reqester, patcher, errorHandler: null);

    public static void FromCacheAndRequest<TState>(IStateFactory stateFactory, IReduxStore<TState> store, StateDb stateDb, Reqester<TState> reqester, Func<Exception, TState?>? errorHandler)
        where TState : class
        => FromCacheAndRequest(stateFactory, store, stateDb, s => s, reqester, (s, _) => s, errorHandler);

    public static void FromCacheAndRequest<TState>(IStateFactory stateFactory, IReduxStore<TState> store, StateDb stateDb, Reqester<TState> reqester)
        where TState : class
        => FromCacheAndRequest(stateFactory, store, stateDb, reqester, errorHandler: null);

    #endregion
}