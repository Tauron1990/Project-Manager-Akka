﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Dsl;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron
{
    public delegate void EnterFlow<in TStart>(TStart msg);

    public sealed class ExternalActorRecieveBuilder<TNext, TStart, TParent, TTarget> : ReceiveBuilderBase<TNext, TStart, TParent>
    {
        public ExternalActorRecieveBuilder([NotNull] ActorFlowBuilder<TStart, TParent> flow, Func<IActorContext, IActorRef> target, bool forward) 
            : base(flow)
        {
            if(forward)
                flow.Register(ad => ad.Receive<TTarget>((msg, context) => target(context).Forward(msg)));
            else
                flow.Register(ad => ad.Receive<TTarget>((msg, context) => target(context).Tell(msg, context.Self)));
        }

        public ExternalActorRecieveBuilder([NotNull] ActorFlowBuilder<TStart, TParent> flow, Func<IActorRef> target, bool forward)
            : base(flow)
        {
            if(forward)
                flow.Register(ad => ad.Receive<TTarget>((msg, context) => target().Forward(msg)));
            else
                flow.Register(ad => ad.Receive<TTarget>((msg, context) => target().Tell(msg, context.Self)));
        }
    }

    [PublicAPI]
    public sealed class RunSelector<TRecieve, TStart, TParent>
    {
        public RunSelector(ActorFlowBuilder<TStart, TParent> flow) 
            => Flow = flow;

        public ActorFlowBuilder<TStart, TParent> Flow { get; }

        public FuncTargetSelector<TRecieve, TNext, TStart, TParent> Func<TNext>(Func<TRecieve, TNext> transformer)
            => new FuncTargetSelector<TRecieve, TNext, TStart, TParent>(Flow, transformer);

        public AsyncFuncTargetSelector<TRecieve, TNext, TStart, TParent> Func<TNext>(Func<TRecieve, Task<TNext>> transformer)
            => new AsyncFuncTargetSelector<TRecieve, TNext, TStart, TParent>(Flow, transformer);

        public ActionFinisher<TRecieve, TStart, TParent> Action(Action<TRecieve> act) 
            => new ActionFinisher<TRecieve, TStart, TParent>(Flow, act);

        public ActionFinisher<TRecieve, TStart, TParent> Action(Func<TRecieve, Task> act) 
            => new ActionFinisher<TRecieve, TStart, TParent>(Flow, act);

        public ExternalActorRecieveBuilder<TRespond, TStart, TParent, TRecieve> External<TRespond>(Func<IActorRef> target, bool forward = false)
            => new ExternalActorRecieveBuilder<TRespond, TStart, TParent, TRecieve>(Flow, target, forward);

        public ExternalActorRecieveBuilder<TRespond, TStart, TParent, TRecieve> External<TRespond>(Func<IActorContext, IActorRef> target, bool forward = false)
            => new ExternalActorRecieveBuilder<TRespond, TStart, TParent, TRecieve>(Flow, target, forward);

        public ActionFinisher<TRecieve, TStart, TParent> External(Func<IActorRef> target, bool forward = false)
        {
            return forward 
                ? new ActionFinisher<TRecieve, TStart, TParent>(Flow, recieve => target().Forward(recieve)) 
                : new ActionFinisher<TRecieve, TStart, TParent>(Flow, recieve => target().Tell(recieve));
        }

        public ActionFinisher<TRecieve, TStart, TParent> External(Func<IActorContext, IActorRef> target, bool forward = false)
        {
            return forward 
                ? new ActionFinisher<TRecieve, TStart, TParent>(Flow, (context, recieve) => target(context).Forward(recieve)) 
                : new ActionFinisher<TRecieve, TStart, TParent>(Flow, (context, recieve) => target(context).Tell(recieve));
        }

        public ActionFinisher<TRecieve, TStart, TParent> External<TTransform>(Func<IActorRef> target, Func<TRecieve, TTransform> convert, bool forward = false)
        {
            return forward
                ? new ActionFinisher<TRecieve, TStart, TParent>(Flow, recieve => target().Forward(convert(recieve)))
                : new ActionFinisher<TRecieve, TStart, TParent>(Flow, recieve => target().Tell(convert(recieve)));
        }

        public ActionFinisher<TRecieve, TStart, TParent> External<TTransform>(Func<IActorContext, IActorRef> target, Func<TRecieve, TTransform> convert, bool forward = false)
        {
            return forward
                ? new ActionFinisher<TRecieve, TStart, TParent>(Flow, (context, recieve) => target(context).Forward(convert(recieve)))
                : new ActionFinisher<TRecieve, TStart, TParent>(Flow, (context, recieve) => target(context).Tell(convert(recieve)));
        }

        public TParent Return() => Flow.Return();
    }

    [PublicAPI]
    public abstract class AbastractTargetSelector<TRespond, TStart, TParent>
    {
        protected AbastractTargetSelector(ActorFlowBuilder<TStart, TParent> flow) => Flow = flow;

        public ActorFlowBuilder<TStart, TParent> Flow { get; }

        public TRespond ToSelf() => ToRef(new RefFunc(RefFuncMode.Self).Send);

        public TRespond ToParent() => ToRef(new RefFunc(RefFuncMode.Parent).Send);

        public TRespond ToSender() => ToRef(new RefFunc(RefFuncMode.Sender).Send);

        public TRespond ToRef(IActorRef actorRef) => ToRef(c => actorRef);

        public abstract TRespond ToRef(Func<IActorContext, IActorRef> actorRef);

        private enum RefFuncMode
        {
            Self,
            Sender,
            Parent
        }

        private class RefFunc
        {
            private readonly RefFuncMode _mode;

            public RefFunc(RefFuncMode mode)
            {
                _mode = mode;
            }

            public IActorRef Send(IActorContext context)
            {
                return _mode switch
                {
                    RefFuncMode.Self => context.Self,
                    RefFuncMode.Sender => context.Sender,
                    RefFuncMode.Parent => context.Parent,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }

    [PublicAPI]
    public sealed class FuncTargetSelector<TRecieve, TNext, TStart, TParent> : AbastractTargetSelector<ReceiveBuilder<TRecieve, TNext, TStart, TParent>, TStart, TParent>
    {
        private readonly Func<TRecieve, TNext> _transformer;

        public FuncTargetSelector(ActorFlowBuilder<TStart, TParent> flow, Func<TRecieve, TNext> transformer)
            : base(flow) =>
            _transformer = transformer;

        public override ReceiveBuilder<TRecieve, TNext, TStart, TParent> ToRef(Func<IActorContext, IActorRef> actorRef) 
            => new ReceiveBuilder<TRecieve, TNext, TStart, TParent>(Flow, actorRef, _transformer);
    }

    [PublicAPI]
    public sealed class AsyncFuncTargetSelector<TRecieve, TNext, TStart, TParent> : AbastractTargetSelector<AyncReceiveBuilder<TRecieve, TNext, TStart, TParent>, TStart, TParent>
    {
        private readonly Func<TRecieve, Task<TNext>> _transformer;

        public AsyncFuncTargetSelector(ActorFlowBuilder<TStart, TParent> flow, Func<TRecieve, Task<TNext>> transformer)
            : base(flow)
        {
            _transformer = transformer;
        }

        public override AyncReceiveBuilder<TRecieve, TNext, TStart, TParent> ToRef(Func<IActorContext, IActorRef> actorRef)
        {
            return new AyncReceiveBuilder<TRecieve, TNext, TStart, TParent>(Flow, actorRef, _transformer);
        }
    }

    [PublicAPI]
    public sealed class ActionFinisher<TRecieve, TStart, TParent>
    {
        private readonly ActorFlowBuilder<TStart, TParent> _flow;

        public ActionFinisher(ActorFlowBuilder<TStart, TParent> flow, Action<TRecieve> runner)
        {
            _flow = flow;
            _flow.Register(a => a.Receive<TRecieve>(new ActionRespond(runner).Run));
        }

        public ActionFinisher(ActorFlowBuilder<TStart, TParent> flow, Func<TRecieve, Task> runner)
        {
            _flow = flow;
            _flow.Register(a => a.ReceiveAsync<TRecieve>(new AsyncActionRespond(runner).Run));
        }

        public ActionFinisher(ActorFlowBuilder<TStart, TParent> flow, Action<IActorContext, TRecieve> runner)
        {
            _flow = flow;
            _flow.Register(a => a.Receive<TRecieve>(new ActionRespondContext(runner).Run));
        }

        public ActionFinisher(ActorFlowBuilder<TStart, TParent> flow, Func<IActorContext, TRecieve, Task> runner)
        {
            _flow = flow;
            _flow.Register(a => a.ReceiveAsync<TRecieve>(new AsyncActionRespondContext(runner).Run));
        }

        public RunSelector<TNew, TStart, TParent> AndRespondTo<TNew>()
        {
            return new RunSelector<TNew, TStart, TParent>(_flow);
        }

        public EnterFlow<TStart> AndBuild()
        {
            return _flow.Build();
        }

        public void AndReceive()
        {
            _flow.Register(e => e.Receive<TStart>(new ReceiveHelper(AndBuild()).Send));
            _flow.BuildReceive();
        }

        public TParent AndReturn()
        {
            return _flow.Return();
        }

        private sealed class ActionRespond
        {
            private readonly Action<TRecieve> _runner;

            public ActionRespond(Action<TRecieve> runner) => _runner = runner;

            public void Run(TRecieve recieve, IActorContext context) => _runner(recieve);
        }

        private sealed class AsyncActionRespond
        {
            private readonly Func<TRecieve, Task> _runner;

            public AsyncActionRespond(Func<TRecieve, Task> runner) => _runner = runner;

            public async Task Run(TRecieve recieve, IActorContext context) => await _runner(recieve);
        }

        private sealed class ActionRespondContext
        {
            private readonly Action<IActorContext, TRecieve> _runner;

            public ActionRespondContext(Action<IActorContext, TRecieve> runner) => _runner = runner;

            public void Run(TRecieve recieve, IActorContext context) => _runner(context, recieve);
        }

        private sealed class AsyncActionRespondContext
        {
            private readonly Func<IActorContext, TRecieve, Task> _runner;

            public AsyncActionRespondContext(Func<IActorContext, TRecieve, Task> runner) => _runner = runner;

            public async Task Run(TRecieve recieve, IActorContext context) => await _runner(context, recieve);
        }

        private sealed class ReceiveHelper
        {
            private readonly EnterFlow<TStart> _invoker;

            public ReceiveHelper(EnterFlow<TStart> invoker)
            {
                _invoker = invoker;
            }

            public void Send(TStart msg, IActorContext context)
            {
                _invoker(msg);
            }
        }
    }

    public abstract class ReceiveBuilderBase<TNext, TStart, TParent>
    {
        protected ReceiveBuilderBase(ActorFlowBuilder<TStart, TParent> flow)
        {
            Flow = flow;
        }

        public ActorFlowBuilder<TStart, TParent> Flow { get; }

        public RunSelector<TNext, TStart, TParent> Then => new RunSelector<TNext, TStart, TParent>(Flow);
    }

    [PublicAPI]
    public class ReceiveBuilder<TReceive, TNext, TStart, TParent> : ReceiveBuilderBase<TNext, TStart, TParent>
    {
        public ReceiveBuilder(ActorFlowBuilder<TStart, TParent> flow, Func<IActorContext, IActorRef> target, Func<TReceive, TNext> transformer)
            : base(flow)
        {
            flow.Register(a => a.Receive<TReceive>(new Receive(target, transformer).Run));
        }

        private sealed class Receive
        {
            private readonly Func<IActorContext, IActorRef> _target;
            private readonly Func<TReceive, TNext> _transformer;

            public Receive(Func<IActorContext, IActorRef> target, Func<TReceive, TNext> transformer)
            {
                _target = target;
                _transformer = transformer;
            }

            public void Run(TReceive rec, IActorContext context)
            {
                var result = _transformer(rec);
                if (result == null) return;
                _target(context).Tell(result, context.Self);
            }
        }

        public void AndReceive()
        {
            Flow.BuildReceive();
        }
    }

    [PublicAPI]
    public class AyncReceiveBuilder<TReceive, TNext, TStart, TParent> : ReceiveBuilderBase<TNext, TStart, TParent>
    {
        public AyncReceiveBuilder(ActorFlowBuilder<TStart, TParent> flow, Func<IActorContext, IActorRef> target, Func<TReceive, Task<TNext>> transformer)
            : base(flow)
        {
            flow.Register(a => a.ReceiveAsync<TReceive>(new Receive(target, transformer).Run));
        }

        private sealed class Receive
        {
            private readonly Func<IActorContext, IActorRef> _target;
            private readonly Func<TReceive, Task<TNext>> _transformer;

            public Receive(Func<IActorContext, IActorRef> target, Func<TReceive, Task<TNext>> transformer)
            {
                _target = target;
                _transformer = transformer;
            }

            public async Task Run(TReceive rec, IActorContext context)
            {
                var result = await _transformer(rec);
                if (result == null) return;
                _target(context).Tell(result, context.Self);
            }
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

        public override ActorFlowBuilder<TStart, TParent> ToRef(Func<IActorContext, IActorRef> actorRef)
        {
            _sendTo(actorRef(Flow.Actor.ExposedContext));
            return _flow;
        }
    }

    [PublicAPI]
    public sealed class ActorFlowBuilder<TStart, TParent>
    {
        private readonly List<Func<EnterFlow<TStart>>> _delgators = new List<Func<EnterFlow<TStart>>>();
        private readonly Action<EnterFlow<TStart>>? _onReturn;
        private readonly TParent _parent;

        private readonly List<Action<IActorDsl>> _recieves = new List<Action<IActorDsl>>();

        private bool _buildReceiveCalled;

        public ActorFlowBuilder(ExposedReceiveActor actor, TParent parent, Action<EnterFlow<TStart>>? onReturn)
        {
            Actor = actor;
            _parent = parent;
            _onReturn = onReturn;
        }

        public ExposedReceiveActor Actor { get; }

        public RunSelector<TStart, TStart, TParent> To => new RunSelector<TStart, TStart, TParent>(this);

        public ActorFlowBuilderTarget<TStart, TParent> Send
            => new ActorFlowBuilderTarget<TStart, TParent>(this, reff => _delgators.Add(() => new Delegator(reff).Tell));

        public TParent Return()
        {
            if (_onReturn != null)
                _onReturn(Build());
            else
                BuildReceive();

            return _parent;
        }

        public void Register(Action<IActorDsl> actorRegister)
        {
            _recieves.Add(actorRegister);
        }


        internal EnterFlow<TStart> Build()
        {
            BuildReceive();
            EnterFlow<TStart>? func = null;
            if (_recieves.Count > 0)
                func = new EntryPoint(Actor).Tell;

            return _delgators.Aggregate(func, (current, delgator) => current.Combine(delgator())) ?? (s => { });
        }

        internal void BuildReceive()
        {
            if (_buildReceiveCalled) return;
            _buildReceiveCalled = true;

            foreach (var recieve in _recieves)
                recieve(Actor);
        }

        private sealed class EntryPoint
        {
            private readonly ExposedReceiveActor _actor;

            public EntryPoint(ExposedReceiveActor actor)
            {
                _actor = actor;
            }

            public void Tell(TStart start)
            {
                if (start == null) return;
                _actor.ExposedContext.Self.Tell(start, _actor.ExposedContext.Self);
            }
        }

        private sealed class Delegator
        {
            private readonly IActorRef _delegator;

            public Delegator(IActorRef delegator)
            {
                _delegator = delegator;
            }

            public void Tell(TStart starter)
            {
                if (starter == null) return;
                _delegator.Tell(starter);
            }
        }
    }

    [PublicAPI]
    public static class ActorFlowExtensions
    {
        public static ActorFlowBuilder<TStart, object> Flow<TStart>(this ExposedReceiveActor actor)
        {
            return new ActorFlowBuilder<TStart, object>(actor, null!, null);
        }
    }
}