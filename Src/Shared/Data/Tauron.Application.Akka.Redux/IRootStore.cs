﻿using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using JetBrains.Annotations;

namespace Tauron.Application.Akka.Redux;

public interface IActionDispatcher
{
    bool CanProcess<TAction>();
    
    bool CanProcess(Type type);
    
    Source<TAction, NotUsed> ObservAction<TAction>()
        where TAction : class;

    Task<IQueueOfferResult> Dispatch(object action);
}

[PublicAPI]
public interface IRootStore : IActionDispatcher, IDisposable
{

    Source<object, NotUsed> ObserveActions();

    IRootStoreState<TState> ForState<TState>()
        where TState : new();
    
    void RegisterMiddlewares(IEnumerable<IMiddleware> middlewares);

    TMiddleware GetMiddleware<TMiddleware>(Func<TMiddleware> factory)
        where TMiddleware : IMiddleware;
    
    void RegisterMiddlewares(params IMiddleware[] middlewares);
}

[PublicAPI]
public interface IRootStoreState<TState>
{
    Source<TResult, NotUsed> ObserveAction<TAction, TResult>(Flow<(TState State, TAction Action), TResult, NotUsed> resultSelector)
        where TAction : class;

    Source<TState, NotUsed> Select();

    Source<TResult, NotUsed> Select<TResult>(Flow<TState, TResult, NotUsed> selector);
}