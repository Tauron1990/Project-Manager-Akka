using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Features
{
    public interface IPreparedFeature : IFeature<AnonymosState>
    {
        KeyValuePair<Type, object> InitialState { get; }
    }

    public sealed record AnonymosState(ImmutableDictionary<Type, object> States)
    {
        public AnonymosState(IEnumerable<IPreparedFeature> features)
            : this(ImmutableDictionary<Type, object>.Empty.AddRange(features.Select(f => f.InitialState))) { }
    }

    [PublicAPI]
    public static class FeatureActorExtensions
    {
        public static IActorRef ActorOf(this IActorRefFactory factory, string? name, params IPreparedFeature[] features) 
            => factory.ActorOf(AnonymosActor.Create(features), name);

        public static IActorRef ActorOf(this IActorRefFactory factory, params IPreparedFeature[] features) 
            => factory.ActorOf(AnonymosActor.Create(features));

        
        private sealed class AnonymosActor : FeatureActorBase<AnonymosActor, AnonymosState>
        {
            public static Props Create(IPreparedFeature[] features)
                => Create(new AnonymosState(features), builder => builder.WithFeatures(features));
        }

    }

    [PublicAPI]
    public static class Feature
    {
        private sealed class PreparedFeature<TState> : ActorBuilder<TState>.ConvertingFeature<TState, AnonymosState>, IPreparedFeature
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

        public static IPreparedFeature[] Create<TState>(IEnumerable<IFeature<TState>> features, Func<TState> stateFunc)
            where TState : notnull
            => features.Select(feature => Create(feature, stateFunc)).ToArray();

        public static IPreparedFeature[] Create<TState>(Func<TState> stateFunc, params IFeature<TState>[] features)
            where TState : notnull
            => features.Select(feature => Create(feature, stateFunc)).ToArray();

        public static IPreparedFeature[] Create(IEnumerable<IFeature<EmptyState>> feature)
            => Create(feature, () => EmptyState.Inst);

        public static IPreparedFeature[] Create(params IFeature<EmptyState>[] feature)
            => Create(feature, () => EmptyState.Inst);

        public static IPreparedFeature[] Create<TState>(IEnumerable<IFeature<TState>> feature, TState state)
            where TState : notnull
            => Create(feature, () => state);

        public static IPreparedFeature[] Create<TState>(TState state, params IFeature<TState>[] feature)
            where TState : notnull
            => Create(feature, () => state);
    }
}