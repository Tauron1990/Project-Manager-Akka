using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.DependencyInjection;
using Akka.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tauron.TAkka;

namespace Tauron.Application.CommonUI.Helper;

[PublicAPI]
public sealed partial class ViewModelSuperviserActor : ObservableActor
{
    private int _count;

    public ViewModelSuperviserActor()
        => Receive<ViewModelSuperviser.CreateModel>(obs => obs.SubscribeWithStatus(NewModel));

    private void NewModel(ViewModelSuperviser.CreateModel obj)
    {
        if(obj.Model.IsInitialized) return;

        _count++;

        Props? props = DependencyResolver.For(Context.System).Props(obj.Model.ModelType);
        IActorRef? actor = Context.ActorOf(props, obj.Name ?? $"{obj.Model.ModelType.Name}--{_count}");

        obj.Model.Init(actor);
    }

    protected override SupervisorStrategy SupervisorStrategy() => new CircuitBreakerStrategy(Log);

    private sealed class CircuitBreakerStrategy : SupervisorStrategy
    {
        private readonly Func<IDecider> _decider;

        private readonly ConcurrentDictionary<IActorRef, IDecider> _deciders = new();

        private CircuitBreakerStrategy(Func<IDecider> decider) => _decider = decider;

        internal CircuitBreakerStrategy(ILogger log)
            : this(() => new CircuitBreakerDecider(log)) { }

        public override IDecider Decider => throw new NotSupportedException("Single Decider not Supportet");

        protected override Directive Handle(IActorRef child, Exception exception)
        {
            // ReSharper disable once HeapView.CanAvoidClosure
            IDecider decider = _deciders.GetOrAdd(child, _ => _decider());

            return decider.Decide(exception);
        }

        public override void ProcessFailure(IActorContext context, bool restart, IActorRef child, Exception cause, ChildRestartStats stats, IReadOnlyCollection<ChildRestartStats> children)
        {
            if(restart)
                RestartChild(child, cause, suspendFirst: false);
            else
                context.Stop(child);
        }

        public override void HandleChildTerminated(IActorContext actorContext, IActorRef child, IEnumerable<IInternalActorRef> children)
        {
            _deciders.TryRemove(child, out _);
        }

        public override ISurrogate ToSurrogate(ActorSystem system)
            => throw new NotSupportedException("Can not serialize CircuitBreakerStrategy");
    }

    internal sealed partial class CircuitBreakerDecider : IDecider
    {
        private readonly ILogger _logger;
        private readonly Stopwatch _time = new();

        private InternalState _currentState = InternalState.Closed;

        private int _restartAtempt;

        internal CircuitBreakerDecider(ILogger log) => _logger = log;

        public Directive Decide(Exception cause)
        {
            switch (cause)
            {
                case ActorInitializationException m:
                    ActorInitializationError(m.InnerException ?? m, m.Actor?.Path.Name ?? "Unkowen");

                    return Directive.Escalate;
                case DeathPactException d:
                    DeathPactError(d);

                    return Directive.Escalate;
                case ActorKilledException:
                    return Directive.Stop;
            }

            UnhandhandeltError(cause);

            switch (_currentState)
            {
                case InternalState.Closed:
                    _time.Restart();
                    _restartAtempt = 1;

                    return Directive.Restart;
                case InternalState.HalfOpen:
                    if(_time.Elapsed > TimeSpan.FromMinutes(2))
                    {
                        _currentState = InternalState.Closed;

                        return Directive.Restart;
                    }

                    _restartAtempt++;

                    _time.Restart();

                    if(_restartAtempt > 6)
                        return Directive.Escalate;

                    _currentState = InternalState.Closed;

                    return Directive.Restart;
                default:
                    return Directive.Escalate;
            }
        }

        [LoggerMessage(EventId = 52, Level = LogLevel.Error, Message = "Initialization Error from Model: {actor}")]
        private partial void ActorInitializationError(Exception ex, string actor);

        [LoggerMessage(EventId = 53, Level = LogLevel.Error, Message = "DeathPactException In Model")]
        private partial void DeathPactError(Exception ex);

        [LoggerMessage(EventId = 54, Level = LogLevel.Error, Message = "Unhandelt Error from Model")]
        private partial void UnhandhandeltError(Exception ex);

        private enum InternalState
        {
            Closed,
            HalfOpen,
        }
    }
}