using Akka;
using Akka.Streams.Dsl;
using JetBrains.Annotations;

namespace Tauron.Application.Akka.Redux;

[PublicAPI]
public interface IRootStoreState<TState>
{
    TState CurrentState { get; }

    Source<TResult, NotUsed> ObservAction<TAction, TResult>(Flow<(TState State, TAction Action), TResult, NotUsed> resultSelector)
        where TAction : class;

    Source<TState, NotUsed> Select();

    Source<TResult, NotUsed> Select<TResult>(Flow<TState, TResult, NotUsed> selector);
}