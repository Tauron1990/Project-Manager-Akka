﻿using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public sealed class EventActor : UntypedActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private readonly Dictionary<Type, Delegate?> _registrations = new();
        private bool _killOnFirstRespond;

        private EventActor(bool killOnFirstRespond) => _killOnFirstRespond = killOnFirstRespond;

        public static IEventActor From(IActorRef actorRef) => new HookEventActor(actorRef);

        public static IEventActor Create(IActorRefFactory system, string? name)
            => Create<Unit>(system, name, null, false);

        public static IEventActor CreateSelfKilling(IActorRefFactory system, string? name)
            => CreateSelfKilling<Unit>(system, name, null);

        public static IEventActor Create<TPayload>(IActorRefFactory system, string? name, Action<TPayload>? handler)
            => Create(system, name, handler, false);

        public static IEventActor CreateSelfKilling<TPayload>(IActorRefFactory system, string? name, Action<TPayload>? handler)
            => Create(system, name, handler, true);

        private static IEventActor Create<TPayload>(IActorRefFactory system, string? name, Action<TPayload>? handler, bool killOnFirstResponse)
        {
            var temp = new HookEventActor(system.ActorOf(Props.Create(() => new EventActor(false)), name));
            if (handler is not null)
                temp.Register(HookEvent.Create(handler)).Ignore();

            return temp;
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case HookEvent hookEvent:
                    if (_registrations.TryGetValue(hookEvent.Target, out var del))
                        del = Delegate.Combine(del, hookEvent.Invoker);
                    else
                        del = hookEvent.Invoker;

                    _registrations[hookEvent.Target] = del;

                    Sender.Tell(Disposable.Create((Self, Del: del, hookEvent.Target), info => info.Self.Tell(new RemoveDel(info.Del, info.Target))));

                    break;
                case RemoveDel remove:
                    if (!_registrations.TryGetValue(remove.Target, out var action)) return;

                    if (action == remove.Delegate)
                        _registrations.Remove(remove.Target);
                    else
                        _registrations[remove.Target] = action.Remove(remove.Delegate);

                    break;
                default:
                    var msgType = message.GetType();
                    if (_registrations.TryGetValue(msgType, out var callDel))
                    {
                        try
                        {
                            callDel?.DynamicInvoke(message);
                        }
                        catch (Exception exception)
                        {
                            _log.Error(exception, "Error On Event Hook Execution");
                        }

                        if (_killOnFirstRespond)
                            Context.Stop(Context.Self);
                    }
                    else
                    {
                        Unhandled(message);
                    }

                    break;
            }
        }

        private sealed record RemoveDel(Delegate Delegate, Type Target);

        private sealed class HookEventActor : IEventActor
        {
            internal HookEventActor(IActorRef actorRef) => OriginalRef = actorRef;

            public IActorRef OriginalRef { get; }

            public Task<IDisposable> Register(HookEvent hookEvent) => OriginalRef.Ask<IDisposable>(hookEvent);

            public void Send(IActorRef actor, object send)
            {
                actor.Tell(send, OriginalRef);
            }
        }
    }
}