using System.Diagnostics;
using Akka.Actor;

namespace Tauron.Features;

[DebuggerStepThrough]
internal sealed class GenericActor : FeatureActorBase<GenericActor, GenericState>
{
    internal static Props Create(IPreparedFeature[] features)
    {
        if(features is [ISimpleFeature simple])
            return simple.MakeProps();

        return Create(
            context => GenericState.Create(context, features),
            builder =>
            {
                foreach (var feature in features)
                    feature.Materialize(builder);
            });
    }
    
    internal static Props Create(IPreparedFeature feature)
    {

        return Create(
            context => GenericState.Create(context, feature),
            builder =>
            {
                feature.Materialize(builder);
            });
    }

    internal static Props Create(IPreparedFeature feature, IPreparedFeature feature1)
    {

        return Create(
            context => GenericState.Create(context, feature, feature1),
            builder =>
            {
                feature.Materialize(builder);
                feature1.Materialize(builder);
            });
    }
    
    internal static Props Create(IPreparedFeature feature, IPreparedFeature feature1, IPreparedFeature feature2)
    {

        return Create(
            context => GenericState.Create(context, feature, feature1, feature2),
            builder =>
            {
                feature.Materialize(builder);
                feature1.Materialize(builder);
                feature2.Materialize(builder);
            });
    }
    
    internal static Props Create(IPreparedFeature feature, IPreparedFeature feature1, IPreparedFeature feature2, IPreparedFeature feature3)
    {

        return Create(
            context => GenericState.Create(context, feature, feature1, feature2, feature3),
            builder =>
            {
                feature.Materialize(builder);
                feature1.Materialize(builder);
                feature2.Materialize(builder);
                feature3.Materialize(builder);
            });
    }
}