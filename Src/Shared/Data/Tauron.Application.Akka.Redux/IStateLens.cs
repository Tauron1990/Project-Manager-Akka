using Akka;
using Akka.Streams.Dsl;
using JetBrains.Annotations;

namespace Tauron.Application.Akka.Redux;

[PublicAPI]
public interface IStateLens<TState, TFeatureState>
{
    IStateLens<TState, TFeatureState> On<TAction>(Flow<(TFeatureState State, TAction Action), TFeatureState, NotUsed> reducer)
        where TAction : class;

    IStateLens<TState, TFeatureState> On<TAction>(Flow<TFeatureState, TFeatureState, NotUsed> reducer)
        where TAction : class;

    IEnumerable<On<TState>> ToList();
}