using Akka;
using Akka.Streams.Dsl;

namespace Tauron.Application.Akka.Redux.Internal;

internal abstract class BaseStateLens<TState, TFeatureState> : IStateLens<TState, TFeatureState>
{
    private readonly List<On<TState>> _ons = new();

    public IStateLens<TState, TFeatureState> On<TAction>(Flow<(TFeatureState State, TAction Action), TFeatureState, NotUsed> reducer) where TAction : class
    {
        _ons.Add(CreateParentReducer(Create.On(reducer)));

        return this;
    }

    public IStateLens<TState, TFeatureState> On<TAction>(Flow<TFeatureState, TFeatureState, NotUsed> reducer) where TAction : class
    {
        _ons.Add(CreateParentReducer(Create.On<TAction, TFeatureState>(reducer)));

        return this;
    }

    public IEnumerable<On<TState>> ToList()
        => _ons.AsEnumerable();

    protected abstract On<TState> CreateParentReducer(On<TFeatureState> on);
}