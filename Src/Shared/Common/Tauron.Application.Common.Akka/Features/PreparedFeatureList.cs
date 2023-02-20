using System.Diagnostics;
using Akka.Actor;

namespace Tauron.Features;

[DebuggerStepThrough]
internal sealed class PreparedFeatureList<TState> : IPreparedFeature
    where TState : notnull
{
    private readonly Func<IUntypedActorContext, TState> _builder;
    private readonly IPreparedFeature[] _target;

    internal PreparedFeatureList(IEnumerable<IPreparedFeature> target, Func<IUntypedActorContext, TState> builder)
    {
        _target = target.ToArray();
        _builder = builder;
    }

    public KeyValuePair<Type, object>? InitialState(IUntypedActorContext context)
        => new(typeof(TState), _builder(context));

    public IEnumerable<IFeature<GenericState>?> Materialize()
        => _target.SelectMany(preparedFeature => preparedFeature.Materialize());
}