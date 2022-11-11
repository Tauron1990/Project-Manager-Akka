using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using Tauron.Application.Akka.Redux.Configuration;

namespace Tauron.Application.Akka.Redux.Internal.Configuration;

public sealed class ReducerFactory<TState> : IReducerFactory<TState>
    where TState : class, new()
{
    private readonly IMaterializer _materializer;

    public ReducerFactory(IMaterializer materializer)
        => _materializer = materializer;

    public On<TState> On<TAction>(Func<TState, TAction, TState> reducer) where TAction : class
        => Create.On(
            Flow.Create<(TState State, TAction Action)>()
               .Select(d => reducer(d.State, d.Action)));

    public On<TState> On<TAction>(Func<TState, TState> reducer) where TAction : class
        => Create.On<TAction, TState>(
            Flow.Create<TState>()
               .Select(reducer));

    public On<TState> On<TAction>(Flow<(TState, TAction), TState, NotUsed> reducer) where TAction : class
        => Create.On(reducer);

    public On<TState> On<TAction>(Flow<TState, TState, NotUsed> reducer) where TAction : class
        => Create.On<TAction, TState>(reducer);

    public IStateLens<TState, TFeatureState> CreateSubReducers<TFeatureState>(Func<TState, TFeatureState> featureSelector, Func<TState, TFeatureState, TState> stateReducer) where TFeatureState : class, new()
        => Create.CreateSubReducers(featureSelector, stateReducer, _materializer);
}