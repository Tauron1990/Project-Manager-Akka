﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.DependencyInjection;
using Akka.Event;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.CommonUI.Model;

namespace Tauron.Application.CommonUI.Helper
{
    public static class ViewModelSuperviserExtensions
    {
        public static void InitModel(this IViewModel model, IActorContext context, string? name = null)
            => ViewModelSuperviser.Get(context.System).Create(model, name);
    }

    public sealed class ViewModelSuperviser
    {
        private static ViewModelSuperviser? _superviser;


        private readonly IActorRef _coordinator;

        private ViewModelSuperviser(IActorRef coordinator) => _coordinator = coordinator;


        public static ViewModelSuperviser Get(ActorSystem system)
        {
            return _superviser ??= new ViewModelSuperviser(system.ActorOf(DependencyResolver.For(system).Props<ViewModelSuperviserActor>(), nameof(ViewModelSuperviser)));
        }

        public void Create(IViewModel model, string? name = null)
        {
            if (model is ViewModelActorRef actualModel)
                _coordinator.Tell(new CreateModel(actualModel, name));
            else
                throw new InvalidOperationException($"Model mot Compatible with {nameof(ViewModelActorRef)}");
        }

        internal sealed class CreateModel
        {
            internal CreateModel(ViewModelActorRef model, string? name)
            {
                Model = model;
                Name = name;
            }

            internal ViewModelActorRef Model { get; }

            internal string? Name { get; }
        }
    }

    [PublicAPI]
    public sealed class ViewModelSuperviserActor : ObservableActor
    {
        private int _count;

        public ViewModelSuperviserActor()
            => Receive<ViewModelSuperviser.CreateModel>(obs => obs.SubscribeWithStatus(NewModel));

        private void NewModel(ViewModelSuperviser.CreateModel obj)
        {
            if (obj.Model.IsInitialized) return;

            _count++;

            var props = DependencyResolver.For(Context.System).Props(obj.Model.ModelType);
            var actor = Context.ActorOf(props, obj.Name ?? $"{obj.Model.ModelType.Name}--{_count}");

            obj.Model.Init(actor);
        }

        protected override SupervisorStrategy SupervisorStrategy() => new CircuitBreakerStrategy(Log);

        private sealed class CircuitBreakerStrategy : SupervisorStrategy
        {
            private readonly Func<IDecider> _decider;

            private readonly ConcurrentDictionary<IActorRef, IDecider> _deciders = new();

            private CircuitBreakerStrategy(Func<IDecider> decider) => _decider = decider;

            internal CircuitBreakerStrategy(ILoggingAdapter log)
                : this(() => new CircuitBreakerDecider(log)) { }

            public override IDecider Decider => throw new NotSupportedException("Single Decider not Supportet");

            protected override Directive Handle(IActorRef child, Exception exception)
            {
                // ReSharper disable once HeapView.CanAvoidClosure
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
            {
                _deciders.TryRemove(child, out _);
            }

            public override ISurrogate ToSurrogate(ActorSystem system)
                => throw new NotSupportedException("Can not serialize CircuitBreakerStrategy");
        }

        private sealed class CircuitBreakerDecider : IDecider
        {
            private readonly ILoggingAdapter _log;
            private readonly Stopwatch _time = new();

            private InternalState _currentState = InternalState.Closed;

            private int _restartAtempt;

            internal CircuitBreakerDecider(ILoggingAdapter log) => _log = log;

            public Directive Decide(Exception cause)
            {
                switch (cause)
                {
                    case ActorInitializationException m:
                        _log.Error(
                            m.InnerException ?? m,
                            "Initialization Error from Model: {Actor}",
                            m.Actor?.Path.Name ?? "Unkowen");

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
                        {
                            _restartAtempt++;
                        }

                        _time.Restart();

                        if (_restartAtempt > 6)
                        {
                            return Directive.Escalate;
                        }
                        else
                        {
                            _currentState = InternalState.Closed;

                            return Directive.Restart;
                        }
                    default:
                        return Directive.Escalate;
                }
            }

            private enum InternalState
            {
                Closed,
                HalfOpen
            }
        }
    }
}