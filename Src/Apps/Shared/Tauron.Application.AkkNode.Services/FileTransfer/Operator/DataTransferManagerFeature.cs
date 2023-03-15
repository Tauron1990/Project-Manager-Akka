using System;
using System.Reactive;
using System.Reactive.Linq;
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
                PendingTransfers.New(),
                Awaiters.New()));

    protected override void ConfigImpl()
    {
        CallSingleHandler = true;
        SupervisorStrategy = new OneForOneStrategy(_ => Directive.Stop);

        Observ<TransferCompled>(obs => obs.Subscribe(m => Context.Stop(Context.Child(m.Event.OperationId.Value))));
        Observ<TransmitRequest>(TransmitRequest);
        Observ<DataTranfer>(ForwardDataTransfer);
        Observ<DataTransferRequest>(RunRequest);
        Observ<IncomingDataTransfer>(HandlerIncommingTransfer);
        Observ<TransferMessage>(TransferMessage);
        Observ<AwaitRequest>(NewAwaitRequest);
        Observ<DeleteAwaiter>(obs => obs.Select(m => m.State with { Awaiters = m.State.Awaiters.Delete(m.Event.Id) }));
    }

    private IObservable<State> NewAwaitRequest(IObservable<StatePair<AwaitRequest, State>> obs)
    {
        return obs.Select(
            p =>
            {
                (AwaitRequest awaitRequest, State state, ITimerScheduler timerScheduler) = p;

                PendingTransfers pendingTransfers = state.PendingTransfers;
                Awaiters awaiters = state.Awaiters;

                pendingTransfers = pendingTransfers.ProcessAwait(
                    p.Sender,
                    awaitRequest,
                    () =>
                    {
                        if(awaitRequest.Timeout is not null)
                            timerScheduler.StartSingleTimer(
                                awaitRequest.Id.Value,
                                new DeleteAwaiter(awaitRequest.Id),
                                awaitRequest.Timeout.Value.ToTimeSpan());

                        awaiters = awaiters.NewWaiter(awaitRequest.Id, p.Sender);
                    });

                return state with { PendingTransfers = pendingTransfers, Awaiters = awaiters };
            });
    }

    private IObservable<Unit> TransferMessage(IObservable<StatePair<TransferMessage, State>> obs)
    {
        return obs.ToUnit(
            m =>
            {
                TransferMessage transferMessage = m.Event;

                m.Context.Child(transferMessage.OperationId.Value)?.Tell(transferMessage);
                TellSelf(new SendEvent(transferMessage, transferMessage.GetType()));
            });
    }

    private IObservable<State> HandlerIncommingTransfer(IObservable<StatePair<IncomingDataTransfer, State>> obs)
    {
        return obs.Select(
            sp =>
            {
                (IncomingDataTransfer incomingDataTransfer, State state) = sp;

                Awaiters awaiters = state.Awaiters;
                PendingTransfers pendingTransfers = state.PendingTransfers;

                awaiters = awaiters.ProcessWaiter(
                    incomingDataTransfer,
                    () => pendingTransfers = pendingTransfers.NewTransfer(incomingDataTransfer));

                return state with { Awaiters = awaiters, PendingTransfers = pendingTransfers };
            });
    }

    private IObservable<Unit> RunRequest(IObservable<StatePair<DataTransferRequest, State>> obs)
    {
        void SelectBuilder(ConditionalSelectBuilder<(IActorRef Child, DataTransferRequest Message), Unit> builder)
        {
            builder.When(
                c => c.Child.IsNobody(),
                start => start.ToUnit(
                    msg => Context.ActorOf<TransferOperatorActor>(msg.Message.OperationId.Value)
                       .Forward(msg.Message)));
            builder.When(
                c => !c.Child.IsNobody(),
                fail => fail.Select(
                        m =>
                        (
                            Target: m.Message.Target.Actor,
                            FailMessage: new TransferFailed(
                                m.Message.OperationId,
                                FailReason.DuplicateOperationId,
                                TransferData.Empty)
                        ))
                   .ToUnit(
                        m =>
                        {
                            m.Target.Tell(m.FailMessage);
                            Self.Tell(m.FailMessage);
                        }));
        }

        return obs.Select(msg => (Child: Context.Child(msg.Event.OperationId.Value), Message: msg.Event))
           .ConditionalSelect()
           .ToResult<Unit>(SelectBuilder);
    }

    private IObservable<State> ForwardDataTransfer(IObservable<StatePair<DataTranfer, State>> obs)
    {
        return obs.Do(m => Context.Child(m.Event.OperationId.Value).Tell(m.Event))
           .Where(m => m.Event is RequestAccept or RequestDeny)
           .Select(m => m.State with { PendingTransfers = m.State.PendingTransfers.Remove(m.Event.OperationId) });
    }

    private IObservable<Unit> TransmitRequest(IObservable<StatePair<TransmitRequest, State>> obs)
    {
        void SelectBuilder(ConditionalSelectBuilder<(IActorRef Child, TransmitRequest Message), Unit> builder)
        {
            builder.When(
                r => r.Child.IsNobody(),
                start => start.ToUnit(r => Context.ActorOf<TransferOperatorActor>(r.Message.OperationId.Value).Tell(r.Message)));
            builder.When(
                r => !r.Child.IsNobody(),
                fail => fail.Select(
                        d => (Message: new TransferFailed(
                                  d.Message.OperationId,
                                  FailReason.DuplicateOperationId,
                                  TransferData.Empty), Sender: d.Message.From))
                   .ToUnit(i => i.Sender.Tell(i.Message)));
        }

        return obs.Select(m => (Child: Context.Child(m.Event.OperationId.Value), Message: m.Event))
           .ConditionalSelect()
           .ToResult<Unit>(SelectBuilder);
    }

    public sealed record State(PendingTransfers PendingTransfers, Awaiters Awaiters);

    private class DeleteAwaiter
    {
        internal DeleteAwaiter(FileOperationId id) => Id = id;
        internal FileOperationId Id { get; }
    }
}