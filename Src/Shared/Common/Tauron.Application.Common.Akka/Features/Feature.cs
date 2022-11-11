using System.Diagnostics;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Features;

[PublicAPI]
[DebuggerStepThrough]
public static class Feature
{
    public static Props Props(params IPreparedFeature[] features)
        => GenericActor.Create(features);

    public static IPreparedFeature Create<TState>(
        Func<IFeature<TState>> feature,
        Func<IUntypedActorContext, TState> stateFunc)
        where TState : notnull
        => new PreparedFeature<TState>(feature, stateFunc);

    public static IPreparedFeature Create(Func<IFeature<EmptyState>> feature)
        => Create(feature, _ => EmptyState.Inst);

    public static IPreparedFeature Create<TState>(Func<IFeature<TState>> feature, TState state)
        where TState : notnull
        => Create(feature, _ => state);

    public static IPreparedFeature Create<TState>(TState state, Func<IFeature<TState>> feature)
        where TState : notnull
        => Create(feature, _ => state);

    public static IPreparedFeature Create<TState>(
        IEnumerable<Func<IFeature<TState>>> features,
        Func<IUntypedActorContext, TState> stateFunc)
        where TState : notnull
        => new PreparedFeatureList<TState>(features.Select(feature => Create(feature, stateFunc)), stateFunc);

    public static IPreparedFeature Create<TState>(
        Func<IUntypedActorContext, TState> stateFunc,
        params Func<IFeature<TState>>[] features)
        where TState : notnull
        => new PreparedFeatureList<TState>(features.Select(feature => Create(feature, stateFunc)), stateFunc);

    public static IPreparedFeature Create(IEnumerable<Func<IFeature<EmptyState>>> feature)
        => Create(feature, _ => EmptyState.Inst);

    public static IPreparedFeature Create(params Func<IFeature<EmptyState>>[] feature)
        => Create(feature, _ => EmptyState.Inst);

    public static IPreparedFeature Create<TState>(IEnumerable<Func<IFeature<TState>>> feature, TState state)
        where TState : notnull
        => Create(feature, _ => state);

    public static IPreparedFeature Create<TState>(TState state, params Func<IFeature<TState>>[] feature)
        where TState : notnull
        => Create(feature, _ => state);
}