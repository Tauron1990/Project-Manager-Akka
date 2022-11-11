using System.Diagnostics;
using Akka.Actor;

namespace Tauron.Features;

[DebuggerStepThrough]
internal sealed class GenericActor : FeatureActorBase<GenericActor, GenericState>
{
    internal static Props Create(IPreparedFeature[] features)
    {
        if(features.Length == 1 && features[0] is ISimpleFeature simple)
            return simple.MakeProps();

        return Create(
            context => new GenericState(features, context),
            builder => builder.WithFeatures(features.SelectMany(feature => feature.Materialize())));
    }
}