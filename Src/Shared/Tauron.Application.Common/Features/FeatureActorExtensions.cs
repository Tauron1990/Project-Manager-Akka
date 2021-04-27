using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Features
{
    public interface IPreparedFeature
    {
        IEnumerable<IFeature<GenericState>> Materialize();

        KeyValuePair<Type, object> InitialState(IUntypedActorContext context);
    }
    [DebuggerStepThrough]
    public sealed record GenericState(ImmutableDictionary<Type, object> States)
    {
        public GenericState(IEnumerable<IPreparedFeature> features, IUntypedActorContext context)
            : this(ImmutableDictionary<Type, object>.Empty.AddRange(features.Select(f => f.InitialState(context))))
        {
        }
    }

    [PublicAPI]
    [DebuggerStepThrough]
    public static class FeatureActorExtensions
    {
        public static IObservable<StatePair<TEvent, TState>> SyncState<TEvent, TState>(
            this IObservable<StatePair<TEvent, TState>> observable, IFeatureActor<TState> actor)
        {
            return observable.ObserveOn(ActorScheduler.From(actor.Self))
                             .Select(evt => evt with {State = actor.CurrentState});
        }

        public static IActorRef ActorOf(this IActorRefFactory factory, string? name, params IPreparedFeature[] features)
            => factory.ActorOf(GenericActor.Create(features), name);

        public static IActorRef ActorOf(this IActorRefFactory factory, params IPreparedFeature[] features)
            => factory.ActorOf(GenericActor.Create(features));

        [DebuggerStepThrough]
        internal sealed class GenericActor : FeatureActorBase<GenericActor, GenericState>
        {
            public static Props Create(IPreparedFeature[] features)
                => Create(c => new GenericState(features, c),
                    builder => builder.WithFeatures(features.SelectMany(f => f.Materialize())));
        }
    }

    [PublicAPI]
    [DebuggerStepThrough]
    public static class Feature
    {
        public static Props Props(params IPreparedFeature[] features)
            => FeatureActorExtensions.GenericActor.Create(features);

        public static IPreparedFeature Create<TState>(Func<IFeature<TState>> feature,
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

        public static IPreparedFeature Create<TState>(IEnumerable<Func<IFeature<TState>>> features,
            Func<IUntypedActorContext, TState> stateFunc)
            where TState : notnull
            => new PreparedFeatureList<TState>(features.Select(feature => Create(feature, stateFunc)), stateFunc);

        public static IPreparedFeature Create<TState>(Func<IUntypedActorContext, TState> stateFunc,
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

        private sealed class FeatureImpl<TState> : ActorBuilder<TState>.ConvertingFeature<TState, GenericState>
            where TState : notnull
        {
            public FeatureImpl(IFeature<TState> target)
                : base(target, state => (TState) state.States[typeof(TState)],
                    (original, state) => original with {States = original.States.SetItem(typeof(TState), state)})
            {
            }
        }
        [DebuggerStepThrough]
        private sealed class PreparedFeature<TState> : IPreparedFeature
            where TState : notnull
        {
            private readonly Func<IFeature<TState>> _feature;
            private readonly Func<IUntypedActorContext, TState> _stateBuilder;

            public PreparedFeature(Func<IFeature<TState>> feature, Func<IUntypedActorContext, TState> stateBuilder)
            {
                _feature = feature;
                _stateBuilder = stateBuilder;
            }

            public IEnumerable<IFeature<GenericState>> Materialize()
            {
                yield return new FeatureImpl<TState>(_feature());
            }

            public KeyValuePair<Type, object> InitialState(IUntypedActorContext c)
                => new(typeof(TState), _stateBuilder(c));
        }
        [DebuggerStepThrough]
        private sealed class PreparedFeatureList<TState> : IPreparedFeature
            where TState : notnull
        {
            private readonly Func<IUntypedActorContext, TState> _builder;
            private readonly IPreparedFeature[] _target;

            public PreparedFeatureList(IEnumerable<IPreparedFeature> target, Func<IUntypedActorContext, TState> builder)
            {
                _target = target.ToArray();
                _builder = builder;
            }

            public KeyValuePair<Type, object> InitialState(IUntypedActorContext context)
                => new(typeof(TState), _builder(context));

            public IEnumerable<IFeature<GenericState>> Materialize()
                => _target.SelectMany(preparedFeature => preparedFeature.Materialize());
        }
    }
}