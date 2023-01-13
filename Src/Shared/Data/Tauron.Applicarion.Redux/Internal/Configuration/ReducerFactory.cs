using Tauron.Applicarion.Redux.Configuration;

namespace Tauron.Applicarion.Redux.Internal.Configuration;

public sealed class ReducerFactory<TState> : IReducerFactory<TState>
    where TState : class, new()
{
    public On<TState> On<TAction>(Func<TState, TAction, TState> reducer) where TAction : class
        => Create.On(reducer);

    public On<TState> On<TAction>(Func<TState, TState> reducer) where TAction : class
        => Create.On<TAction, TState>(reducer);

    public IStateLens<TState, TFeatureState> CreateSubReducers<TFeatureState>(Func<TState, TFeatureState> featureSelector, Func<TState, TFeatureState, TState> stateReducer) where TFeatureState : class, new()
        => Create.CreateSubReducers(featureSelector, stateReducer);
}