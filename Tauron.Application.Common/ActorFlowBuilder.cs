﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron
{
    public delegate void EnterFlow<in TStart>(TStart msg);

    [PublicAPI]
    public sealed class RunSelector<TRecieve, TStart, TParent>
    {
        public ActorFlowBuilder<TStart, TParent> Flow { get; }

        public RunSelector(ActorFlowBuilder<TStart, TParent> flow) => Flow = flow;

        public FuncTargetSelector<TRecieve, TNext, TStart, TParent> Func<TNext>(Func<TRecieve, TNext> transformer)
            => new FuncTargetSelector<TRecieve, TNext, TStart, TParent>(Flow, transformer);

        public AsyncFuncTargetSelector<TRecieve, TNext, TStart, TParent> Func<TNext>(Func<TRecieve, Task<TNext>> transformer)
            => new AsyncFuncTargetSelector<TRecieve, TNext, TStart, TParent>(Flow, transformer);

        public ActionFinisher<TRecieve, TStart, TParent> Action(Action<TRecieve> act)
            => new ActionFinisher<TRecieve, TStart, TParent>(Flow, act);

        public ActionFinisher<TRecieve, TStart, TParent> Action(Func<TRecieve, Task> act)
            => new ActionFinisher<TRecieve, TStart, TParent>(Flow, act);

        public TParent Return() => Flow.Return();
    }

    [PublicAPI]
    public abstract class AbastractTargetSelector<TRespond, TStart, TParent>
    {
        public ActorFlowBuilder<TStart, TParent> Flow { get; }

        protected AbastractTargetSelector(ActorFlowBuilder<TStart, TParent> flow) => Flow = flow;

        public TRespond ToSelf()
            => ToRef(Flow.Actor.ExposedContext.Self);

        public TRespond ToParent()
            => ToRef(Flow.Actor.ExposedContext.Parent);

        public TRespond ToSender()
            => ToRef(Flow.Actor.ExposedContext.Sender);

        public abstract TRespond ToRef(IActorRef actorRef);
    }

    [PublicAPI]
    public sealed class FuncTargetSelector<TRecieve, TNext, TStart, TParent> : AbastractTargetSelector<ReceiveBuilder<TRecieve, TNext, TStart, TParent>, TStart, TParent>
    {
        private readonly Func<TRecieve, TNext> _transformer;

        public FuncTargetSelector(ActorFlowBuilder<TStart, TParent> flow, Func<TRecieve, TNext> transformer)
            : base(flow) =>
            _transformer = transformer;

        public override ReceiveBuilder<TRecieve, TNext, TStart, TParent> ToRef(IActorRef actorRef)
            => new ReceiveBuilder<TRecieve, TNext, TStart, TParent>(Flow, actorRef, _transformer);
    }

    [PublicAPI]
    public sealed class AsyncFuncTargetSelector<TRecieve, TNext, TStart, TParent> : AbastractTargetSelector<AyncReceiveBuilder<TRecieve, TNext, TStart, TParent>, TStart, TParent>
    {
        private readonly Func<TRecieve, Task<TNext>> _transformer;

        public AsyncFuncTargetSelector(ActorFlowBuilder<TStart, TParent> flow, Func<TRecieve, Task<TNext>> transformer)
            : base(flow) =>
            _transformer = transformer;

        public override AyncReceiveBuilder<TRecieve, TNext, TStart, TParent> ToRef(IActorRef actorRef)
            => new AyncReceiveBuilder<TRecieve, TNext, TStart, TParent>(Flow, actorRef, _transformer);
    }

    [PublicAPI]
    public sealed class ActionFinisher<TRecieve, TStart, TParent>
    {
        private sealed class ActionRespond
        {
            private readonly Action<TRecieve> _runner;

            public ActionRespond(Action<TRecieve> runner) => _runner = runner;

            public void Run(TRecieve recieve, IActorContext context)
                => _runner(recieve);
        }

        private sealed class AsyncActionRespond
        {
            private readonly Func<TRecieve, Task> _runner;

            public AsyncActionRespond(Func<TRecieve, Task> runner) => _runner = runner;

            public async Task Run(TRecieve recieve, IActorContext context) => await _runner(recieve);
        }

        private readonly ActorFlowBuilder<TStart, TParent> _flow;

        public ActionFinisher(ActorFlowBuilder<TStart, TParent> flow, Action<TRecieve> runner)
        {
            _flow = flow;
            _flow.Register(a => a.Exposed.Receive<TRecieve>(new ActionRespond(runner).Run));
        }

        public ActionFinisher(ActorFlowBuilder<TStart, TParent> flow, Func<TRecieve, Task> runner)
        {
            _flow = flow;
            _flow.Register(a => a.Exposed.ReceiveAsync<TRecieve>(new AsyncActionRespond(runner).Run));
        }

        public EnterFlow<TStart> Build()
            => _flow.Build();

        public void Receive()
            => _flow.BuildReceive();

        public TParent Return() => _flow.Return();
    }


    public abstract class ReceiveBuilderBase<TNext, TStart, TParent>
    {
        public ActorFlowBuilder<TStart, TParent> Flow { get; }

        protected ReceiveBuilderBase(ActorFlowBuilder<TStart, TParent> flow) => Flow = flow;

        public RunSelector<TNext, TStart, TParent> Then => new RunSelector<TNext, TStart, TParent>(Flow);
    }

    [PublicAPI]
    public class ReceiveBuilder<TReceive, TNext, TStart, TParent> : ReceiveBuilderBase<TNext, TStart, TParent>
    {
        private sealed class Receive
        {
            private readonly IActorRef _target;
            private readonly Func<TReceive, TNext> _transformer;

            public Receive(IActorRef target, Func<TReceive, TNext> transformer)
            {
                _target = target;
                _transformer = transformer;
            }

            public void Run(TReceive rec, IActorContext context)
            {
                var result = _transformer(rec);
                if(result == null) return;
                _target.Tell(result, context.Self);
            }
        }

        public ReceiveBuilder(ActorFlowBuilder<TStart, TParent> flow, IActorRef target, Func<TReceive, TNext> transformer)
            : base(flow)
        {
            flow.Register(a => a.Exposed.Receive<TReceive>(new Receive(target, transformer).Run));
        }
    }

    [PublicAPI]
    public class AyncReceiveBuilder<TReceive, TNext, TStart, TParent> : ReceiveBuilderBase<TNext, TStart, TParent>
    {
        private sealed class Receive
        {
            private readonly IActorRef _target;
            private readonly Func<TReceive, Task<TNext>> _transformer;

            public Receive(IActorRef target, Func<TReceive, Task<TNext>> transformer)
            {
                _target = target;
                _transformer = transformer;
            }

            public async Task Run(TReceive rec, IActorContext context)
            {
                var result = await _transformer(rec);
                if(result == null) return;
                _target.Tell(result, context.Self);
            }
        }

        public AyncReceiveBuilder(ActorFlowBuilder<TStart, TParent> flow, IActorRef target, Func<TReceive, Task<TNext>> transformer)
            : base(flow)
        {
            flow.Register(a => a.Exposed.ReceiveAsync<TReceive>(new Receive(target, transformer).Run));
        }
    }

    public class ActorFlowBuilderTarget<TStart, TParent> : AbastractTargetSelector<ActorFlowBuilder<TStart, TParent>, TStart, TParent>
    {
        private readonly ActorFlowBuilder<TStart, TParent> _flow;
        private readonly Action<IActorRef> _sendTo;

        public ActorFlowBuilderTarget([NotNull] ActorFlowBuilder<TStart, TParent> flow, Action<IActorRef> sendTo) 
            : base(flow)
        {
            _flow = flow;
            _sendTo = sendTo;
        }

        public override ActorFlowBuilder<TStart, TParent> ToRef(IActorRef actorRef)
        {
            _sendTo(actorRef);
            return _flow;
        }
    }

    [PublicAPI]
    public sealed class ActorFlowBuilder<TStart, TParent>
    {
        private readonly TParent _parent;
        private readonly Action<EnterFlow<TStart>>? _onReturn;

        public ActorFlowBuilder(ExposedReceiveActor actor, TParent parent, Action<EnterFlow<TStart>>? onReturn)
        {
            Actor = actor;
            _parent = parent;
            _onReturn = onReturn;
        }

        public TParent Return()
        {
            if(_onReturn != null)
                _onReturn(Build());
            else
                BuildReceive();

            return _parent;
        }

        private sealed class EntryPoint
        {
            private readonly ExposedReceiveActor _actor;

            public EntryPoint(ExposedReceiveActor actor) => _actor = actor;

            public void Tell(TStart start)
                => _actor.ExposedContext.Self.Tell(start, _actor.ExposedContext.Self);
        }

        private sealed class Delegator
        {
            private readonly IActorRef _delegator;

            public Delegator(IActorRef delegator) => _delegator = delegator;

            public void Tell(TStart starter)
                => _delegator.Tell(starter);
        }

        private readonly List<Action<ExposedReceiveActor>> _recieves = new List<Action<ExposedReceiveActor>>();
        private readonly List<Func<EnterFlow<TStart>>> _delgators = new List<Func<EnterFlow<TStart>>>();

        public ExposedReceiveActor Actor { get; }

        public RunSelector<TStart, TStart, TParent> To => new RunSelector<TStart, TStart, TParent>(this);

        public ActorFlowBuilderTarget<TStart, TParent> Send 
            => new ActorFlowBuilderTarget<TStart, TParent>(this, reff => _delgators.Add(() => new Delegator(reff).Tell));

        public void Register(Action<ExposedReceiveActor> actorRegister)
            => _recieves.Add(actorRegister);


        internal EnterFlow<TStart> Build()
        {
            BuildReceive();
            EnterFlow<TStart>? func = null;
            if(_recieves.Count > 0)
                func = new EntryPoint(Actor).Tell;

            return _delgators.Aggregate(func, (current, delgator) => current.Combine(delgator())) ?? (s => { });
        }

        internal void BuildReceive()
        {
            foreach (var recieve in _recieves)
                recieve(Actor);
        }
    }

    [PublicAPI]
    public static class ActorFlowExtensions
    {
        public static ActorFlowBuilder<TStart, object> Flow<TStart>(this ExposedReceiveActor actor)
            => new ActorFlowBuilder<TStart, object>(actor, null!, null);
    }
}