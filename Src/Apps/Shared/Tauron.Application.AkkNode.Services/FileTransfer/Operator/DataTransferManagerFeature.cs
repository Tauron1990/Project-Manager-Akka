using System;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Akka.Actor;
using Tauron.Features;
using Tauron.ObservableExt;
using static Tauron.Application.AkkaNode.Services.FileTransfer.TransferMessages;

namespace Tauron.Application.AkkaNode.Services.FileTransfer.Operator;

public sealed class DataTransferManagerFeature : ActorFeatureBase<DataTransferManagerFeature.State>
{
    public static IPreparedFeature New()
        => Feature.Create(
            () => new DataTransferManagerFeature(),
            _ => new State(
                ImmutableDictionary<string, IncomingDataTransfer>.Empty,
                ImmutableDictionary<string, AwaitRequestInternal>.Empty));

    protected override void ConfigImpl()
    {
        CallSingleHandler = true;
        SupervisorStrategy = new OneForOneStrategy(_ => Directive.Stop);

        Receive<TransferCompled>().Subscribe(m => Context.Stop(Context.Child(m.OperationId)));

        Receive<TransmitRequest>(TransmitRequest);

        Receive<DataTranfer>(
            obs => obs
               .Do(m => Context.Child(m.Event.OperationId).Tell(m.Event))
               .Where(m => m.Event is RequestAccept or RequestDeny)
               .Select(m => m.State with { PendingTransfers = m.State.PendingTransfers.Remove(m.Event.OperationId) }));

        Receive<DataTransferRequest>(
            obs => obs
               .Select(msg => new { Child = Context.Child(msg.Event.OperationId), Message = msg.Event })
               .ConditionalSelect()
               .ToResult<Unit>(
                    b =>
                    {
                        b.When(
                            c => c.Child.IsNobody(),
                            start => start.ToUnit(
                                msg
                                    => Context.ActorOf<TransferOperatorActor>(msg.Message.OperationId).Forward(msg.Message)));
                        b.When(
                            c => !c.Child.IsNobody(),
                            fail => fail.Select(
                                    m => new
                                         {
                                             Target = m.Message.Target.Actor,
                                             FailMessage = new TransferFailed(m.Message.OperationId, FailReason.DuplicateOperationId, null)
                                         })
                               .ToUnit(
                                    m =>
                                    {
                                        m.Target.Tell(m.FailMessage);
                                        Self.Tell(m.FailMessage);
                                    }));
                    }));

        Receive<IncomingDataTransfer>(
            obs => obs.Select(
                sp =>
                {
                    var (incomingDataTransfer, state, _) = sp;

                    State newState = state;

                    if (state.Awaiters.TryGetValue(incomingDataTransfer.OperationId, out var awaitRequest))
                    {
                        awaitRequest.Target.Tell(new AwaitResponse(incomingDataTransfer));
                        newState = newState with { Awaiters = newState.Awaiters.Remove(incomingDataTransfer.OperationId) };
                    }
                    else
                    {
                        newState = newState with
                                   {
                                       PendingTransfers = newState.PendingTransfers.SetItem(incomingDataTransfer.OperationId, incomingDataTransfer)
                                   };
                    }

                    return newState;
                }));

        Receive<TransferMessage>(
            obs => obs.ToUnit(
                m =>
                {
                    var (transferMessage, _, _) = m;

                    Context.Child(transferMessage.OperationId)?.Tell(transferMessage);
                    TellSelf(new SendEvent(transferMessage, transferMessage.GetType()));
                }));

        Receive<AwaitRequest>(
            obs => obs.Select(
                p =>
                {
                    var (awaitRequest, state, timerScheduler) = p;

                    if (state.PendingTransfers.TryGetValue(awaitRequest.Id, out var income))
                    {
                        Sender.Tell(new AwaitResponse(income));

                        return state with { PendingTransfers = state.PendingTransfers.Remove(awaitRequest.Id) };
                    }

                    if (Timeout.InfiniteTimeSpan != awaitRequest.Timeout)
                        timerScheduler.StartSingleTimer(awaitRequest.Id, new DeleteAwaiter(awaitRequest.Id), awaitRequest.Timeout);

                    return state with
                           {
                               Awaiters = state.Awaiters.SetItem(awaitRequest.Id, new AwaitRequestInternal(Sender))
                           };
                }));

        Receive<DeleteAwaiter>(obs => obs.Select(m => m.State with { Awaiters = m.State.Awaiters.Remove(m.Event.Id) }));
    }

    private IObservable<Unit> TransmitRequest(IObservable<StatePair<TransmitRequest, State>> obs)
        => obs.Select(m => (Child: Context.Child(m.Event.OperationId), Message: m.Event))
           .ConditionalSelect()
           .ToResult<Unit>(
                b =>
                {
                    b.When(
                        r => r.Child.IsNobody(),
                        start => start.ToUnit(r => Context.ActorOf<TransferOperatorActor>(r.Message.OperationId).Tell(r.Message)));
                    b.When(
                        r => !r.Child.IsNobody(),
                        fail => fail.Select(d => new
                                                 {
                                                     Message = new TransferFailed(d.Message.OperationId, FailReason.DuplicateOperationId, null),
                                                     Sender = d.Message.From
                                                 })
                           .ToUnit(i => i.Sender.Tell(i.Message)));
                });

    public sealed record State(ImmutableDictionary<string, IncomingDataTransfer> PendingTransfers, ImmutableDictionary<string, AwaitRequestInternal> Awaiters);

    private class DeleteAwaiter
    {
        internal DeleteAwaiter(string id) => Id = id;
        internal string Id { get; }
    }

    public class AwaitRequestInternal
    {
        public AwaitRequestInternal(IActorRef target) => Target = target;
        public IActorRef Target { get; }
    }
}