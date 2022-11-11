using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Features;

[PublicAPI]
[DebuggerStepThrough]
public sealed class ActorBuilder<TState>
{
    private readonly Action<IFeature<TState>> _registrar;

    public ActorBuilder(Action<IFeature<TState>> registrar) => _registrar = registrar;

    #pragma warning disable AV1551
    public ActorBuilder<TState> WithFeature(IFeature<TState> feature)
        #pragma warning restore AV1551
    {
        _registrar(feature);

        return this;
    }

    public ActorBuilder<TState> WithFeatures(IEnumerable<IFeature<TState>> features)
    {
        foreach (var feature in features)
            _registrar(feature);

        return this;
    }

    public ActorBuilder<TState> WithFeature<TNewState>(
        IFeature<TNewState> feature, Func<TState, TNewState> convert,
        Func<TState, TNewState, TState> convertBack)
        => WithFeature(new ConvertingFeature<TNewState, TState>(feature, convert, convertBack));

    [DebuggerStepThrough]
    internal class ConvertingFeature<TTarget, TOriginal> : IFeature<TOriginal>
    {
        private readonly Func<TOriginal, TTarget> _convert;
        private readonly Func<TOriginal, TTarget, TOriginal> _convertBack;
        private readonly IFeature<TTarget> _feature;

        internal ConvertingFeature(
            IFeature<TTarget> feature, Func<TOriginal, TTarget> convert,
            Func<TOriginal, TTarget, TOriginal> convertBack)
        {
            _feature = feature;
            _convert = convert;
            _convertBack = convertBack;
        }

        public IEnumerable<string> Identify() => _feature.Identify();

        public virtual void Init(IFeatureActor<TOriginal> actor)
            => _feature.Init(new StateDelegator<TTarget, TOriginal>(actor, _convert, _convertBack));

        public void PostStop()
            => _feature.PostStop();

        public void PreStart()
            => _feature.PreStart();

        void IDisposable.Dispose() => _feature.Dispose();

        void IResourceHolder.AddResource(IDisposable res) => _feature.AddResource(res);

        void IResourceHolder.RemoveResource(IDisposable res) => _feature.RemoveResource(res);
    }

    [DebuggerStepThrough]
    private sealed class StateDelegator<TTarget, TOriginal> : IFeatureActor<TTarget>
    {
        private readonly Func<TOriginal, TTarget> _convert;
        private readonly Func<TOriginal, TTarget, TOriginal> _convertBack;
        private readonly IFeatureActor<TOriginal> _original;

        internal StateDelegator(
            IFeatureActor<TOriginal> original, Func<TOriginal, TTarget> convert,
            Func<TOriginal, TTarget, TOriginal> convertBack)
        {
            _original = original;
            _convert = convert;
            _convertBack = convertBack;
        }

        #pragma warning disable AV1551
        public IObservable<TEvent> Receive<TEvent>()
            #pragma warning restore AV1551
            => _original.Receive<TEvent>();

        public IActorRef Self
            => _original.Self;

        public IActorRef Parent
            => _original.Parent;

        public IActorRef Sender
            => _original.Sender;

        public IUntypedActorContext Context
            => _original.Context;

        bool IFeatureActor<TTarget>.CallSingleHandler
        {
            get => _original.CallSingleHandler;
            set => _original.CallSingleHandler = value;
        }

        public SupervisorStrategy? SupervisorStrategy
        {
            get => _original.SupervisorStrategy;
            set => _original.SupervisorStrategy = value;
        }

        public void UpdateState(TTarget state)
            => _original.UpdateState(_convertBack(_original.CurrentState, state));

        public void TellSelf(object msg) => _original.TellSelf(msg);

        public IDisposable Subscribe(IObserver<TTarget> observer) => _original.Select(_convert).Subscribe(observer);

        public IObservable<IActorContext> Start => _original.Start;
        public IObservable<IActorContext> Stop => _original.Stop;
        public TTarget CurrentState => _convert(_original.CurrentState);

        public IObservable<TSignal> WaitForSignal<TSignal>(TimeSpan timeout, Predicate<TSignal> match) => _original.WaitForSignal(timeout, match);

        public ITimerScheduler Timers
        {
            get => _original.Timers;
            set => _original.Timers = value;
        }

        public void Dispose() => _original.Dispose();

        public void AddResource(IDisposable res) => _original.AddResource(res);

        public void RemoveResource(IDisposable res) => _original.RemoveResource(res);

        #pragma warning disable AV1551
        public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TTarget>>, IObservable<Unit>> handler)
            => _original.Receive<TEvent>(obs => handler(obs.Select(statePair => statePair.Convert(_convert))));

        public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TTarget>>, IObservable<TTarget>> handler)
            => _original.Receive<TEvent>(
                obs
                    => handler(obs.Select(statePair => statePair.Convert(_convert)))
                       .Select(state => _convertBack(_original.CurrentState, state)));

        public void Receive<TEvent>(
            Func<IObservable<StatePair<TEvent, TTarget>>, IObservable<Unit>> handler,
            Func<Exception, bool> errorHandler)
            => _original.Receive<TEvent>(obs => handler(obs.Select(statePair => statePair.Convert(_convert))), errorHandler);

        public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TTarget>>, IDisposable> handler)
            => _original.Receive<TEvent>(obs => handler(obs.Select(statePair => statePair.Convert(_convert))));
        #pragma warning restore AV1551
    }
}