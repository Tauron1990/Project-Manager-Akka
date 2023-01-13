using System.Collections.Immutable;
using System.Diagnostics;
using Akka.Actor;

namespace Tauron.Features;

[DebuggerStepThrough]
public sealed record GenericState(ImmutableDictionary<Type, object> States)
{
    public GenericState(IEnumerable<IPreparedFeature> features, IUntypedActorContext context)
        : this(ImmutableDictionary<Type, object>.Empty.AddRange(features.Select(feature => feature.InitialState(context)))) { }
}