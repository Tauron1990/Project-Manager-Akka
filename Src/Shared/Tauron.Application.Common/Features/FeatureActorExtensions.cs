using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Features
{
    public interface IPreparedFeature : IFeature<GenericState>
    {
        KeyValuePair<Type, object> InitialState { get; }
    }

    public sealed record GenericState(ImmutableDictionary<Type, object> States)
    {
        public GenericState(IEnumerable<IPreparedFeature> features)
            : this(ImmutableDictionary<Type, object>.Empty.AddRange(features.Select(f => f.InitialState))) { }
    }

    [PublicAPI]
    public static class FeatureActorExtensions
    {
        public static IActorRef ActorOf(this IActorRefFactory factory, string? name, params IPreparedFeature[] features) 
            => factory.ActorOf(GenericActor.Create(features), name);

        public static IActorRef ActorOf(this IActorRefFactory factory, params IPreparedFeature[] features) 
            => factory.ActorOf(GenericActor.Create(features));

        
        internal sealed class GenericActor : FeatureActorBase<GenericActor, GenericState>
        {
            public static Props Create(IPreparedFeature[] features)
                => Create(new GenericState(features), builder => builder.WithFeatures(features));
        }

    }

    [PublicAPI]
    public static class Feature
    {
        private sealed class PreparedFeature<TState> : ActorBuilder<TState>.ConvertingFeature<TState, GenericState>, IPreparedFeature
            where TState : notnull
        {
            private readonly Func<TState> _builder;

            public PreparedFeature(IFeature<TState> target, Func<TState> builder)
                : base(target, state => (TState)state.States[typeof(TState)], (original, state) => original with{ States = original.States.SetItem(typeof(TState), state)} )
            {
                _builder = builder;
            }
            
            public KeyValuePair<Type, object> InitialState => new(typeof(TState), _builder());
        }

        private sealed class PreparedFeatureList<TState> : IPreparedFeature
            where TState : notnull
        {
            private readonly IPreparedFeature[] _target;
            private readonly Func<TState> _builder;

            public PreparedFeatureList(IEnumerable<IPreparedFeature> target, Func<TState> builder)
            {
                _target = target.ToArray();
                _builder = builder;
            }

            public IEnumerable<string> Identify() => _target.SelectMany(preparedFeature => preparedFeature.Identify());

            public void Init(IFeatureActor<GenericState> actor)
            {
                _target.Foreach(f => f.Init(actor));    
            }

            public KeyValuePair<Type, object> InitialState => new(typeof(TState), _builder());
        }

        public static Props Props(params IPreparedFeature[] features)
            => FeatureActorExtensions.GenericActor.Create(features);

        public static IPreparedFeature Create<TState>(IFeature<TState> feature, Func<TState> stateFunc)
            where TState : notnull
            => new PreparedFeature<TState>(feature, stateFunc);

        public static IPreparedFeature Create(IFeature<EmptyState> feature)
            => Create(feature, () => EmptyState.Inst);

        public static IPreparedFeature Create<TState>(IFeature<TState> feature, TState state)
            where TState : notnull
            => Create(feature, () => state);

        public static IPreparedFeature Create<TState>(TState state, IFeature<TState> feature)
            where TState : notnull
            => Create(feature, () => state);

        public static IPreparedFeature Create<TState>(IEnumerable<IFeature<TState>> features, Func<TState> stateFunc)
            where TState : notnull
            => new PreparedFeatureList<TState>(features.Select(feature => Create(feature, stateFunc)), stateFunc);

        public static IPreparedFeature Create<TState>(Func<TState> stateFunc, params IFeature<TState>[] features)
            where TState : notnull
            => new PreparedFeatureList<TState>(features.Select(feature => Create(feature, stateFunc)), stateFunc);

        public static IPreparedFeature Create(IEnumerable<IFeature<EmptyState>> feature)
            => Create(feature, () => EmptyState.Inst);

        public static IPreparedFeature Create(params IFeature<EmptyState>[] feature)
            => Create(feature, () => EmptyState.Inst);

        public static IPreparedFeature Create<TState>(IEnumerable<IFeature<TState>> feature, TState state)
            where TState : notnull
            => Create(feature, () => state);

        public static IPreparedFeature Create<TState>(TState state, params IFeature<TState>[] feature)
            where TState : notnull
            => Create(feature, () => state);
    }
}