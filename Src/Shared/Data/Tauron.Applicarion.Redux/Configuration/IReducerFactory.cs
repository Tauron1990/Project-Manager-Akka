using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux.Configuration;

[PublicAPI]
public interface IReducerFactory<TState>
    where TState : class, new()
{
    On<TState> On<TAction>(Func<TState, TAction, TState> reducer) where TAction : class;

    On<TState> On<TAction>(Func<TState, TState> reducer) where TAction : class;


    IStateLens<TState, TFeatureState> CreateSubReducers<TFeatureState>(Func<TState, TFeatureState> featureSelector, Func<TState, TFeatureState, TState> stateReducer)
        where TFeatureState : class, new();
}