using System.Reactive.Linq;
using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux.Extensions;

public delegate TSelect Selector<in TState, out TSelect>(TState toSelect);

public delegate Task<TData> Reqester<TData>(TData input);

public delegate TState Patcher<in TData, TState>(TData data, TState state);

[PublicAPI]
public class DynamicSource
{
    #region Async

    public static void FromAsync<TState>(IStore<TState> store, Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => store.RegisterEffects(
            Create.Effect<TState>(
                () =>
                    Observable.FromAsync(source)
                       .Select(d => MutateCallback.Create(() => d))
                       .OnErrorResumeNext(e => errorHandler is null 
                                              ? Observable.Empty<MutateCallback<TState>>() 
                                              : Observable.Return(errorHandler(e))
                                                 .NotNull()
                                                 .Select(s => MutateCallback.Create(() => s)))));

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
        Func<Exception, TState>? errorHandler)
        => () => CreateRequestObservable(store, selector, reqester, patcher, errorHandler);

    private static IObservable<TState> CreateRequestObservable<TState, TSelect>(
        IStore<TState> store,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher,
        Func<Exception, TState>? errorHandler)
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
           .OnErrorResumeNext(e => errorHandler is null ? Observable.Empty<TState>() : Observable.Return(errorHandler(e)));

    public static void Request<TState, TSelect>(
        IStore<TState> store,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher,
        Func<Exception, TState>? errorHandler)
        => store.RegisterEffects(
            Create.Effect<TState>(
                s => CreateRequestObservable(s, selector, reqester, patcher, errorHandler)
                   .Select(state => MutateCallback.Create(() => state))));

    public static void Request<TState, TSelect>(
        IStore<TState> store,
        Selector<TState, TSelect> selector,
        Reqester<TSelect> reqester,
        Patcher<TSelect, TState> patcher)
        => Request(store, selector, reqester, patcher, null);
    
    public static void Request<TState>(IStore<TState> store, Reqester<TState> reqester, Func<Exception, TState>? errorHandler)
        => store.RegisterEffects(
            Create.Effect<TState>(
                s => CreateRequestObservable(s, state => state, reqester, (state, _) => state, errorHandler)
                   .Select(state => MutateCallback.Create(() => state))));

    public static void Request<TState>(IStore<TState> store, Reqester<TState> reqester)
        => Request(store, reqester, null);

    #endregion
}