﻿using System.Reactive;
using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.Stage;
using Akka.Util;
using JetBrains.Annotations;
using Stl.Fusion;
using Tauron.Application.Akka.Redux.Extensions.Cache;

namespace Tauron.Application.Akka.Redux.Extensions;

[PublicAPI]
public static class DynamicSource
{
    #region Async

    private static Source<TState, NotUsed> CreateAsyncStateSource<TState>(Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => Source.Single(Unit.Default)
           .SelectAsync(1, _ => source())
           .Recover(
                ex =>
                (
                    errorHandler is null
                        ? Option<TState?>.None
                        : new Option<TState?>(errorHandler(ex))
                )!)
           .Where(state => state is not null);

    public static void FromAsync<TState>(IReduxStore<TState> store, Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => store.RegisterEffects(
            Create.Effect<TState>(
                () => CreateAsyncStateSource(source, errorHandler)
                   .Select(s => (object)MutateCallback.Create(s))));

    public static void FromAsync<TState>(IReduxStore<TState> store, Task<TState> source, Func<Exception, TState?>? errorHandler)
        => FromAsync(store, () => source, errorHandler);

    public static void FromAsync<TState>(IReduxStore<TState> store, Func<Task<TState>> source)
        => FromAsync(store, source, errorHandler: null);

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
                TState currentState = store.CurrentState;
                TSelect data = selector(currentState);
                token.ThrowIfCancellationRequested();
                TSelect requestResult = await reqester(token, data).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                TState patch = patcher(requestResult, currentState);
                token.ThrowIfCancellationRequested();

                return patch;
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                return errorHandler?.Invoke(e) ?? default(TState);
            }
        }

        var state = stateFactory.NewComputed<TState?>(RunRequest);

        return DynamicUpdate.ToSource(state, skipErrors: true).Where(s => s is not null)!;
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
            Create.Effect<TState>(
                s => CreateRequestSource(stateFactory, s, selector, reqester, patcher, errorHandler)
                   .Select(state => (object)MutateCallback.Create(state))));

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
            Create.Effect<TState>(
                s => CreateRequestSource(stateFactory, s, state => state, reqester, (state, _) => state, errorHandler)
                   .Select(state => (object)MutateCallback.Create(state))));

    public static void FromRequest<TState>(IStateFactory stateFactory, IReduxStore<TState> store, Reqester<TState> reqester)
        where TState : class
        => FromRequest(stateFactory, store, reqester, errorHandler: null);

    #endregion

    #region Cache

    private static Source<TState, NotUsed> CreateCacheSource<TState>(StateDb stateDb, Func<Exception, TState?>? errorHandler)
        => Source.FromGraph(GraphDsl.Create(b => b.Add(new StateDbFetcher<TState>(stateDb, errorHandler))));

    private sealed class StateDbFetcher<TState> : GraphStage<SourceShape<TState>>
    {
        private readonly Func<Exception, TState?>? _handler;

        private readonly Outlet<TState> _outlet = new("StateDbFetcher.out");

        private readonly StateDb _stateDb;

        internal StateDbFetcher(StateDb stateDb, Func<Exception, TState?>? handler)
        {
            _stateDb = stateDb;
            _handler = handler;
            Shape = new SourceShape<TState>(_outlet);
        }

        public override SourceShape<TState> Shape { get; }

        protected override GraphStageLogic CreateLogic(Attributes inheritedAttributes)
            => new Logic(this);

        private sealed class Logic : GraphStageLogic
        {
            private readonly Func<Exception, TState?>? _handler;

            private readonly Outlet<TState> _outlet;
            private readonly StateDb _stateDb;

            internal Logic(StateDbFetcher<TState> fetcher) : base(fetcher.Shape)
            {
                _stateDb = fetcher._stateDb;
                _handler = fetcher._handler;
                _outlet = fetcher._outlet;

                SetHandler(_outlet, RunFetch);
            }

            private async void RunFetch()
            {
                TState? potenialState;

                try
                {
                    potenialState = await _stateDb.Get<TState>().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    if(_handler is null)
                    {
                        FailStage(e);

                        return;
                    }

                    try
                    {
                        potenialState = _handler(e);

                    }
                    catch (Exception exception)
                    {
                        FailStage(exception);

                        return;
                    }
                }

                if(potenialState is null)
                {
                    CompleteStage();
                }
                else
                {
                    Push(_outlet, potenialState);
                    CompleteStage();
                }
            }
        }
    }

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
        => store.RegisterEffects(
            Create.Effect<TState>(
                () => CreateCacheSource(stateDb, errorHandler)
                   .Select(state => (object)MutateCallback.Create(state))));

    public static void FromCache<TState>(IReduxStore<TState> store, StateDb stateDb)
        => FromCache(store, stateDb, errorHandler: null);

    public static void FromCacheAndAsync<TState>(IReduxStore<TState> store, StateDb stateDb, Func<Task<TState>> source, Func<Exception, TState?>? errorHandler)
        => store.RegisterEffects(
            Create.Effect<TState>(
                () => CreateCacheAsyncObservable(stateDb, source, errorHandler)
                   .Select(state => (object)MutateCallback.Create(state))));

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
                () => CreateCacheRequestSource(stateFactory, store, stateDb, selector, reqester, patcher, errorHandler)
                   .Select(state => (object)MutateCallback.Create(state))));

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