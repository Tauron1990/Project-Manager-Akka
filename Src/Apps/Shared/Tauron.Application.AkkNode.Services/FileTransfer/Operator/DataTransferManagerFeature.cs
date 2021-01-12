using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using JetBrains.Annotations;
using Akka.Actor;
using Tauron.Features;
using Tauron.ObservableExt;
using Dm = Tauron.Application.AkkNode.Services.FileTransfer.TransferMessages;

namespace Tauron.Application.AkkNode.Services.FileTransfer.Operator
{
    public sealed class DataTransferManagerFeature : IFeature<DataTransferManagerFeature.State>
    {
        public sealed record State(ImmutableDictionary<string, IncomingDataTransfer> PendingTransfers, ImmutableDictionary<string, AwaitRequestInternal> Awaiters);

        public static IPreparedFeature New()
            => Feature.Create(new DataTransferManagerFeature(), () => new State(ImmutableDictionary<string, IncomingDataTransfer>.Empty, ImmutableDictionary<string, AwaitRequestInternal>.Empty));

        public void Init(IFeatureActor<State> actor)
        {
            actor.SupervisorStrategy = new OneForOneStrategy(_ => Directive.Stop);

            actor.Receive<Dm.TransmitRequest>(obs => obs.Select(m => new {Child = actor.Context.Child(m.Event.OperationId), Message = m.Event})
                                                        .ConditionalSelect()
                                                        .ToResult<Unit>(b =>
                                                                        {
                                                                            b.When(r => r.Child.IsNobody(),
                                                                                   start => start.ToUnit(r => actor.Context.ActorOf<TransferOperatorActor>(r.Message.OperationId).Tell(r.Message)));
                                                                            b.When(r => !r.Child.IsNobody(),
                                                                                   fail => fail.Select(d => new
                                                                                                            {
                                                                                                                Message = new TransferFailed(d.Message.OperationId, FailReason.DuplicateOperationId, null),
                                                                                                                Sender = d.Message.From

                                                                                                            })
                                                                                               .ToUnit(i => i.Sender.Tell(i.Message)));
                                                                        }));
            actor.Receive<Dm.DataTranfer>(obs => obs.Where(m => m.Event is Dm.RequestAccept && m.Event is Dm.RequestDeny)
                                                    .Do(m => actor.Context.Child(m.Event.OperationId).Tell(m.Event))
                                                    .Select(m => m.State with {PendingTransfers = m.State.PendingTransfers.Remove(m.Event.OperationId)}));
            actor.Receive<DataTransferRequest>(obs =>)
            Flow<DataTransferRequest>(b =>
                                          b.Action(r =>
                                                   {
                                                       var op = Context.Child(r.OperationId);
                                                       if (!op.Equals(ActorRefs.Nobody))
                                                       {
                                                           r.Target.Actor.Tell(new TransferFailed(r.OperationId, FailReason.DuplicateOperationId, null));
                                                           Self.Tell(new TransferFailed(r.OperationId, FailReason.DuplicateOperationId, null));
                                                           return;
                                                       }

                                                       Context.ActorOf(Props.Create<TransferOperatorActor>(), r.OperationId).Forward(r);
                                                   }));
        }

        private class DeleteAwaiter
        {
            public string Id { get; }

            public DeleteAwaiter(string id) => Id = id;
        }

        public class AwaitRequestInternal
        {
            public IActorRef Target { get; }

            public AwaitRequestInternal(IActorRef target) => Target = target;
        }
    }


    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class DataTransferManagerActorOld : ExpandedReceiveActor, IWithTimers
    {
        





            Flow<IncomingDataTransfer>(b => b.Action(dt =>
            {
                if (_awaiters.TryGetValue(dt.OperationId, out var awaitRequest))
                {
                    awaitRequest.Target.Tell(new AwaitResponse(dt));
                    _awaiters.Remove(dt.OperationId);
                }
                else
                    _pendingTransfers[dt.OperationId] = dt;

                subscribe.Send(dt);
            }));

            Flow<TransferMessages.TransferCompled>(b =>
                b.Action(tc =>
                {
                    Context.Stop(Context.Child(tc.OperationId));
                    subscribe.Send(tc, tc.GetType());
                }));

            Flow<TransferMessages.TransferMessage>(b =>
                b.Action(tm =>
                {
                    Context.Child(tm.OperationId).Tell(tm);
                    subscribe.Send(tm, tm.GetType());
                }));

            Flow<AwaitRequest>(b => b.Action(r =>
            {
                if (_pendingTransfers.TryGetValue(r.Id, out var income))
                {
                    Sender.Tell(income);
                    _pendingTransfers.Remove(r.Id);
                }
                else
                {
                    _awaiters[r.Id] = new DataTransferManagerFeature.AwaitRequestInternal(Sender);
                    if (Timeout.InfiniteTimeSpan != r.Timeout)
                        Timers.StartSingleTimer(r.Id, new DeleteAwaiter(r.Id), r.Timeout);
                }
            }));

            Flow<DeleteAwaiter>(b => b.Action(d => _awaiters.Remove(d.Id)));

            subscribe.MakeReceive();
        }

        protected override SupervisorStrategy SupervisorStrategy() => new OneForOneStrategy(e => Directive.Stop);
    }
}