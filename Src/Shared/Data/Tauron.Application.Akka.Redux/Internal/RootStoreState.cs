using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;

namespace Tauron.Application.Akka.Redux.Internal;

public sealed class RootStoreState<TState> : IRootStoreState<TState>, IActionDispatcher, IInternalRootStoreState<TState>
{
    public RootStoreState(IReduxStore<TState> store)
        => Store = store;

    public bool CanProcess<TAction>()
        => Store.CanProcess<TAction>();

    public bool CanProcess(Type type)
        => Store.CanProcess(type);

    public Source<TAction, NotUsed> ObservAction<TAction>() where TAction : class
        => Store.ObservAction<TAction>();

    public Task<IQueueOfferResult> Dispatch(object action)
        => Store.Dispatch(action);

    public Sink<object, NotUsed> Dispatcher()
        => Store.Dispatcher();

    public IReduxStore<TState> Store { get; }

    public TState CurrentState => Store.CurrentState;

    public Source<TResult, NotUsed> ObservAction<TAction, TResult>(Flow<(TState State, TAction Action), TResult, NotUsed> resultSelector) where TAction : class
        => Store.ObservAction(resultSelector);

    public Source<TState, NotUsed> Select()
        => Store.Select();

    public Source<TResult, NotUsed> Select<TResult>(Flow<TState, TResult, NotUsed> selector)
        => Store.Select(selector);
}