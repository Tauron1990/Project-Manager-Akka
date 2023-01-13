using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux;

[PublicAPI]
public interface IStateLens<TState, TFeatureState>
{
    IStateLens<TState, TFeatureState> On<TAction>(Func<TFeatureState, TAction, TFeatureState> featureReducer)
        where TAction : class;

    IStateLens<TState, TFeatureState> On<TAction>(Func<TFeatureState, TFeatureState> featureReducer)
        where TAction : class;

    IEnumerable<On<TState>> ToList();
}