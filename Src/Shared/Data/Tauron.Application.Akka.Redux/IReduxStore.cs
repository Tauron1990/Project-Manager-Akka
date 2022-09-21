using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using JetBrains.Annotations;

namespace Tauron.Application.Akka.Redux;

public sealed record DispatchedAction<TState>(TState State, object Action);

public sealed record ActionState<TState, TAction>(TState State, TAction Action);

[PublicAPI]
public interface IReduxStore<TState> : IActionDispatcher, IDisposable
{
    IMaterializer Materializer { get; }
   
    Source<TResult, NotUsed> Select<TResult>(Flow<TState, TResult, NotUsed> selector);

    Source<TState, NotUsed> Select();

    Source<ActionState<TState, TAction>, NotUsed> ObservActionState<TAction>();

    Source<TResult, NotUsed> ObservAction<TAction, TResult>(Flow<(TState State, TAction Action), TResult, NotUsed> selector);

    TState CurrentState { get; }

    void Reset();

    void RegisterReducers(IEnumerable<On<TState>> reducers);
    
    void RegisterEffects(IEnumerable<Effect<TState>> effects);
    
    
    void RegisterReducers(params On<TState>[] reducers);
    
    void RegisterEffects(params Effect<TState>[] effects);
    
    Source<object, NotUsed> ObserveAction();
    
    Source<TAction, NotUsed> ObserveAction<TAction>()
        where TAction : class;

    Source<TResult, NotUsed> ObserveAction<TAction, TResult>(Flow<(TAction Action, TState State), TResult, NotUsed> resultSelector)
        where TAction : class;
}