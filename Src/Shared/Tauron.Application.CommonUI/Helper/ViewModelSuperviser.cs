using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.DI.Core;
using Akka.Event;
using Akka.Util;
using Tauron.Akka;
using Tauron.Application.CommonUI.Model;

namespace Tauron.Application.CommonUI.Helper
{
    public static class ViewModelSuperviserExtensions
    {
        public static void InitModel(this IViewModel model, IUntypedActorContext context, string? name = null)
            => ViewModelSuperviser.Get(context.System).Create(model, name);
    }

    public sealed class ViewModelSuperviser
    {
        private static ViewModelSuperviser? _superviser;


        public static ViewModelSuperviser Get(ActorSystem system)
            => _superviser ??= new ViewModelSuperviser(system.ActorOf(system.DI().Props<ViewModelSuperviserActor>(), nameof(ViewModelSuperviser)));


        private readonly IActorRef _coordinator;

        private ViewModelSuperviser(IActorRef coordinator) => _coordinator = coordinator;

        public void Create(IViewModel model, string? name = null)
        {
            if (model is ViewModelActorRef actualModel)
                _coordinator.Tell(new CreateModel(actualModel, name));
            else
                throw new InvalidOperationException($"Model mot Compatible with {nameof(ViewModelActorRef)}");
        }

        internal sealed class CreateModel
        {
            public ViewModelActorRef Model { get; }

            public string? Name { get; }

            public CreateModel(ViewModelActorRef model, string? name)
            {
                Model = model;
                Name = name;
            }
        }
    }

    public sealed class ViewModelSuperviserActor : ExpandedReceiveActor
    {
        private int _count;

        public ViewModelSuperviserActor()
        {
            Receive<ViewModelSuperviser.CreateModel>(NewModel);
        }

        private void NewModel(ViewModelSuperviser.CreateModel obj)
        {
            if (obj.Model.IsInitialized) return;

            _count++;

            var props = Context.System.DI().Props(obj.Model.ModelType);
            var actor = Context.ActorOf(props, obj.Name ?? $"{obj.Model.ModelType.Name}--{_count}");

            obj.Model.Init(actor);
        }

        protected override SupervisorStrategy SupervisorStrategy() 
            => new CircuitBreakerStrategy(Log);

        private sealed class CircuitBreakerStrategy : SupervisorStrategy
        {
            private readonly Func<IDecider> _decider;

            private readonly ConcurrentDictionary<IActorRef, IDecider> _deciders = new();

            private CircuitBreakerStrategy(Func<IDecider> decider) 
                => _decider = decider;

            public CircuitBreakerStrategy(ILoggingAdapter log)
                : this(() => new CircuitBreakerDecider(log))
            {
                
            }

            protected override Directive Handle(IActorRef child, Exception exception)
            {
                var decider = _deciders.GetOrAdd(child, _ => _decider());
                return decider.Decide(exception);
            }

            public override void ProcessFailure(IActorContext context, bool restart, IActorRef child, Exception cause, ChildRestartStats stats, IReadOnlyCollection<ChildRestartStats> children)
            {
                if (restart)
                    RestartChild(child, cause, false);
                else
                    context.Stop(child);
            }

            public override void HandleChildTerminated(IActorContext actorContext, IActorRef child, IEnumerable<IInternalActorRef> children) 
                => _deciders.TryRemove(child, out _);

            public override ISurrogate ToSurrogate(ActorSystem system) 
                => throw new NotSupportedException("Can not serialize CircuitBreakerStrategy");

            public override IDecider Decider => throw new NotSupportedException("Single Decider not Supportet");
        }

        private sealed class CircuitBreakerDecider : IDecider
        {
            private readonly ILoggingAdapter _log;
            private readonly Stopwatch _time = new();

            private InternalState _currentState = InternalState.Closed;
            
            private int _restartAtempt;

            public CircuitBreakerDecider(ILoggingAdapter log) => _log = log;

            public Directive Decide(Exception cause)
            {
                switch (cause)
                {
                    case ActorInitializationException m:
                        _log.Error(m.InnerException ?? m, "Initialization Error from Model: {Actor}", m.Actor?.Path.Name ?? "Unkowen");
                        return Directive.Escalate;
                    case DeathPactException d:
                        _log.Error(d, "DeathPactException In Model");
                        return Directive.Escalate;
                    case ActorKilledException:
                        return Directive.Stop;
                }

                _log.Error(cause, "Unhandled Error from Model");

                switch (_currentState)
                {
                    case InternalState.Closed:
                        _time.Restart();
                        _restartAtempt = 1;
                        return Directive.Restart;
                    case InternalState.HalfOpen:
                        if (_time.Elapsed > TimeSpan.FromMinutes(2))
                        {
                            _currentState = InternalState.Closed;
                            return Directive.Restart;
                        }
                        else
                            _restartAtempt++;

                        _time.Restart();

                        if (_restartAtempt > 6)
                            return Directive.Escalate;
                        else
                        {
                            _currentState = InternalState.Closed;
                            return Directive.Restart;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private enum InternalState
            {
                Closed,
                HalfOpen,
            }
        }
    }
}