﻿using Stl.Fusion;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions;
using Tauron.Operations;

namespace Tauron.Applicarion.Redux.Internal.Configuration;

public sealed class RequesterFactory<TState> : IRequestFactory<TState> where TState : class, new()
{
    private readonly IErrorHandler _errorHandler;

    //private readonly List<Action<IReduxStore<TState>>> _registrar;
    private readonly List<Action<IRootStore, IReduxStore<TState>>> _registrar;
    private readonly IStateFactory _stateFactory;

    public RequesterFactory(List<Action<IRootStore, IReduxStore<TState>>> registrar, IErrorHandler errorHandler, IStateFactory stateFactory)
    {
        _registrar = registrar;
        _errorHandler = errorHandler;
        _stateFactory = stateFactory;
    }

    public IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, ValueTask<SimpleResult>> runRequest, Func<TState, TAction, TState> onScess) where TAction : class
    {
        AddRequestInternal(Adept(runRequest), onScess, null);

        return this;
    }

    public IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, ValueTask<SimpleResult>> runRequest, Func<TState, TAction, TState> onScess, Func<TState, object, TState> onFail) where TAction : class
    {
        AddRequestInternal(Adept(runRequest), onScess, onFail);

        return this;
    }

    public IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, Task<SimpleResult>> runRequest, Func<TState, TAction, TState> onScess) where TAction : class
    {
        AddRequestInternal(Adept(runRequest), onScess, null);

        return this;
    }

    public IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, Task<SimpleResult>> runRequest, Func<TState, TAction, TState> onScess, Func<TState, object, TState> onFail) where TAction : class
    {
        AddRequestInternal(Adept(runRequest), onScess, onFail);

        return this;
    }

    public IRequestFactory<TState> OnTheFlyUpdate<TSource, TData>(Func<TState, TSource> sourceSelector, Func<CancellationToken, Func<CancellationToken, ValueTask<TSource>>, Task<TData>> fetcher, Func<TState, TData, TState> patcher)
    {
        _registrar.Add(
            (_, s) =>
            {
                DynamicUpdate.OnTheFlyUpdate(
                    s,
                    _stateFactory,
                    CreateSourceSelector(sourceSelector),
                    fetcher,
                    CreatePatacher(patcher),
                    _errorHandler.RequestError);
            });

        return this;
    }

    public IRequestFactory<TState> OnTheFlyUpdate<TData>(Func<CancellationToken, Task<TData>> fetcher, Func<TState, TData, TState> patcher)
    {
        _registrar.Add(
            (_, s) => DynamicUpdate.OnTheFlyUpdate(s, _stateFactory, fetcher, CreatePatacher(patcher), _errorHandler.RequestError));

        return this;
    }

    private static Func<TState, object, TState> GetDefaultErrorHandler(IErrorHandler errorHandler)
        => (state, o) =>
           {
               switch (o)
               {
                   case string text:
                       errorHandler.RequestError(text);

                       break;
                   case Exception exception:
                       errorHandler.RequestError(exception);

                       break;
               }

               return state;
           };

    private static Func<TAction, ValueTask<SimpleResult>> Adept<TAction>(Func<TAction, CancellationToken, ValueTask<SimpleResult>> request)
        => async a => await TimeoutToken.WithDefault(CancellationToken.None, async c => await request(a, c).ConfigureAwait(false))
              .ConfigureAwait(false);

    private static Func<TAction, ValueTask<SimpleResult>> Adept<TAction>(Func<TAction, CancellationToken, Task<SimpleResult>> request)
        => async a => await TimeoutToken.WithDefault(CancellationToken.None, async c => await request(a, c).ConfigureAwait(false))
              .ConfigureAwait(false);


    private static Func<TState, object, TState> Adept(IErrorHandler errorHandler, Func<TState, object, TState>? updater)
        => updater ?? GetDefaultErrorHandler(errorHandler);

    private void AddRequestInternal<TAction>(Func<TAction, ValueTask<SimpleResult>> runRequest, Func<TState, TAction, TState> onScess, Func<TState, object, TState>? onFail)
        where TAction : class
    {
        Func<TAction, ValueTask<SimpleResult>> CreateRunner(Func<TAction, ValueTask<SimpleResult>> from)
            => async action => await from(action).ConfigureAwait(false);

        _registrar.Add(
            (s, _) => DynamicUpdate.AddRequest(s, CreateRunner(runRequest), onScess, Adept(_errorHandler, onFail)));
    }

    private static Selector<TState, TSource> CreateSourceSelector<TSource>(Func<TState, TSource> selector)
        => state => selector(state);

    private static Patcher<TData, TState> CreatePatacher<TData>(Func<TState, TData, TState> patcher)
        => (data, state) => patcher(state, data);
}