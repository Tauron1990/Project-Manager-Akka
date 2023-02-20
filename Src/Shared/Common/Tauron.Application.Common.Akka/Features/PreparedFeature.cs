using System.Diagnostics;
using Akka.Actor;

namespace Tauron.Features;

[DebuggerStepThrough]
internal sealed class PreparedFeature<TState> : IPreparedFeature, ISimpleFeature
    where TState : notnull
{
    private readonly Func<IFeature<TState>> _feature;
    private readonly Func<IUntypedActorContext, TState> _stateBuilder;

    internal PreparedFeature(Func<IFeature<TState>> feature, Func<IUntypedActorContext, TState> stateBuilder)
    {
        _feature = feature;
        _stateBuilder = stateBuilder;
    }

    public IEnumerable<IFeature<GenericState>?> Materialize()
    {
        yield return new FeatureImpl<TState>(_feature());
    }

    public KeyValuePair<Type, object>? InitialState(IUntypedActorContext c)
        => new(typeof(TState), _stateBuilder(c));

    public Props MakeProps()
        => SimpleFeatureActor<TState>.Create(_stateBuilder, builder => builder.WithFeature(_feature()));
}