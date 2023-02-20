using System.Collections.Immutable;
using System.Diagnostics;
using Akka.Actor;

namespace Tauron.Features;

[DebuggerStepThrough]
public sealed record GenericState(ImmutableDictionary<Type, object> States)
{
    //features.Select(feature => feature.InitialState(context))

    public GenericState(IEnumerable<IPreparedFeature> features, IUntypedActorContext context)
        : this(
            ImmutableDictionary<Type, object>.Empty.AddRange(
                from feature in features
                let state = feature.InitialState(context)
                where state is not null
                select state.Value))
    {
    }
}