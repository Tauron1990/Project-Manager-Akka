using System;
using System.Buffers;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;
using Tauron.TAkka;
using static Tauron.Application.AkkaNode.Services.FileTransfer.TransferMessages;

namespace Tauron.Application.AkkaNode.Services.FileTransfer.Operator;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TransferOperatorActor : FSM<OperatorState, OperatorData>
{
    #if DATADEBUG
        private static readonly TimeSpan StartTimeout = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan InitTimeout = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan SendingTimeout = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan FailedTimeout = TimeSpan.FromMinutes(30);
    #else
    private static readonly TimeSpan StartTimeout = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan InitTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan SendingTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan FailedTimeout = TimeSpan.FromSeconds(30);
    #endif

    private readonly ILoggingAdapter _log = Context.GetLogger();
    private NextChunk? _lastChunk;

    private byte[]? _outgoningBytes;
    private int _sendingAttempts;

    public TransferOperatorActor()
    {
        StartWith(OperatorState.Waiting, new OperatorData());

        When(OperatorState.Waiting, ProcessAsWaiting);
        When(OperatorState.InitSending, ProcessAsInitSending, StartTimeout);
        When(OperatorState.InitReciving, ProcessAsInitReciving, InitTimeout);
        When(OperatorState.Sending, ProcessSending, SendingTimeout);
        When(OperatorState.Reciving, ProcessReciving, SendingTimeout);
        When(OperatorState.Failed, ProcessFailing, FailedTimeout);
        When(OperatorState.Compled, _ => null);

        OnTransition(WhenTransition);
        WhenUnhandled(UnHandledEvent);

        Initialize();
    }

    private State<OperatorState, OperatorData> UnHandledEvent(Event<OperatorData> state)
    {
        switch (state.FsmEvent)
        {
            case StateTimeout:
                _log.Error("Trisnmission Timeout {Id}", GetId(state));

                return GoTo(OperatorState.Failed)
                   .Using(state.StateData.Failed(Parent, FailReason.Timeout, null));
            case TransferError error:
                _log.Warning("Incoming Transfer Failed {Id}", GetId(state));

                return GoTo(OperatorState.Failed).Using(state.StateData.InComingError(error));
            case DataTranfer:
                _log.Warning("Incorrect DataTransfer Event {Id}", GetId(state));

                return GoTo(OperatorState.Failed)
                   .Using(state.StateData.Failed(Parent, FailReason.ComunicationError, null));
            default:
                _log.Warning("Unkown or Incorrect message Incming {Type}", state.FsmEvent.GetType());

                return Stay();
        }
    }

    private void WhenTransition(OperatorState _, OperatorState nextState)
    {
        if(nextState != OperatorState.Failed && nextState != OperatorState.Compled)
            return;

        NextStateData.TransferStrem.Dispose();
        if(_outgoningBytes != null)
            ArrayPool<byte>.Shared.Return(_outgoningBytes);
    }

    private State<OperatorState, OperatorData> ProcessFailing(Event<OperatorData> state)
    {
        _log.Warning("Transfer Failed {Id}", GetId(state));

        void Set(TransferCompled failed)
        {
            state.StateData.Completion?.SetResult(failed);
            if(state.StateData.SendBack)
                state.StateData.Sender.Tell(failed);
            if(state.StateData.IndividualMessage != null)
                state.StateData.Sender.Tell(state.StateData.IndividualMessage);
            Parent.Tell(failed);
        }

        switch (state.FsmEvent)
        {
            case TransferError error:
            {
                Set(error.ToFailed());

                break;
            }
            case StateTimeout:
                Set(new TransferFailed(GetId(state), FailReason.Timeout, TransferData.Empty));

                break;
            default:
            {
                TransferError manmesg = state.StateData.Error ?? new TransferError(
                    (state.FsmEvent as TransferMessage)?.OperationId ?? state.StateData.OperationId,
                    FailReason.CorruptState,
                    TransferData.Empty);

                state.StateData.TargetManager.Tell(manmesg, Parent);
                Set(manmesg.ToFailed());

                break;
            }
        }

        return Stay();
    }

    private State<OperatorState, OperatorData>? ProcessReciving(Event<OperatorData> state)
    {
        return state.FsmEvent switch
        {
            NextChunk chunk => NextChunkRecived(state, chunk),
            RequestDeny => Stay(),
            _ => null,
        };
    }

    private State<OperatorState, OperatorData>? NextChunkRecived(Event<OperatorData> state, NextChunk chunk)
    {
        try
        {
            uint reciveCrc = OperatorData.Crc32.ComputeChecksum(chunk.Data, chunk.Count);

            if(reciveCrc == chunk.Crc)
                return ProcessChunk(state, chunk);

            state.StateData.TargetManager.Tell(new RepeadChunk(state.StateData.OperationId), Parent);

            return Stay();

        }
        catch (Exception e)
        {
            _log.Error(e, "Error on Write Stream");

            return GoTo(OperatorState.Failed)
               .Using(state.StateData.Failed(Parent, FailReason.WriteError, e.Message));
        }
    }

    private State<OperatorState, OperatorData> ProcessChunk(Event<OperatorData> state, NextChunk chunk)
    {
        if(chunk.Count > 0)
            state.StateData.TransferStrem.Write(chunk.Data, 0, chunk.Count);

        if(chunk.Finish)
        {
            OperatorData? data = state.StateData;
            try
            {
                if(data.TransferStrem.WriteCrc != chunk.FinishCrc)
                    return GoTo(OperatorState.Failed)
                       .Using(state.StateData.Failed(Parent, FailReason.ComunicationError, null));

                data.TargetManager.Tell(new SendingCompled(state.StateData.OperationId));

                var msg = new TransferSucess(state.StateData.OperationId, state.StateData.Metadata ?? TransferData.Empty);
                state.StateData.Completion?.SetResult(msg);
                Parent.Tell(msg);

                return GoTo(OperatorState.Compled);
            }
            finally
            {
                data.TransferStrem.Dispose();
            }
        }

        state.StateData.TargetManager.Tell(new SendNextChunk(state.StateData.OperationId));

        return Stay();
    }

    private State<OperatorState, OperatorData>? ProcessSending(Event<OperatorData> state)
    {
        return state.FsmEvent switch
        {
            SendNextChunk => Sending(state),
            StartTrensfering => Sending(state),
            SendingCompled => SendingCompletd(state),
            RepeadChunk => RepeadingChunk(state),
            _ => null,
        };
    }

    private State<OperatorState, OperatorData>? RepeadingChunk(Event<OperatorData> state)
    {
        _sendingAttempts += 1;

        if(_sendingAttempts > 5)
            return GoTo(OperatorState.Failed)
               .Using(state.StateData.Failed(Parent, FailReason.ToManyResends, null));

        state.StateData.TargetManager.Tell(_lastChunk, Parent);

        return Stay();
    }

    private State<OperatorState, OperatorData>? SendingCompletd(Event<OperatorData> state)
    {
        state.StateData.TransferStrem.Dispose();
        if(_outgoningBytes != null)
            ArrayPool<byte>.Shared.Return(_outgoningBytes);
        _outgoningBytes = null;

        var comp = new TransferSucess(state.StateData.OperationId, state.StateData.Metadata ?? TransferData.Empty);
        Parent.Tell(comp);
        if(state.StateData.SendBack)
            state.StateData.Sender.Tell(comp);
        if(state.StateData.IndividualMessage != null)
            state.StateData.Sender.Tell(state.StateData.IndividualMessage);

        return GoTo(OperatorState.Compled);
    }

    private State<OperatorState, OperatorData>? Sending(Event<OperatorData> state)
    {
        _outgoningBytes ??= ArrayPool<byte>.Shared.Rent(1024 * 1024);
        try
        {
            _sendingAttempts = 0;
            int count = state.StateData.TransferStrem.Read(_outgoningBytes, 0, _outgoningBytes.Length);
            bool last = count == 0;
            uint crc = OperatorData.Crc32.ComputeChecksum(_outgoningBytes, count);

            state.StateData.TargetManager.Tell(
                _lastChunk = new NextChunk(state.StateData.OperationId, _outgoningBytes, count, last, crc, state.StateData.TransferStrem.ReadCrc),
                Parent);

            return Stay();
        }
        catch (Exception e)
        {
            _log.Error(e, "Error on Read Stream or Sending");

            return GoTo(OperatorState.Failed)
               .Using(state.StateData.Failed(Parent, FailReason.ReadError, e.Message));
        }
    }

    private State<OperatorState, OperatorData>? ProcessAsInitReciving(Event<OperatorData> state)
    {
        switch (state.FsmEvent)
        {
            case RequestDeny deny:
                _log.Info("Tranfer Request Deny {Id}", state.StateData.OperationId);
                state.StateData.TargetManager.Tell(deny, Parent);

                return GoTo(OperatorState.Failed);
            case RequestAccept accept:
                _log.Info("Request Accepted {Id}", GetId(state));
                try
                {
                    var newState = GoTo(OperatorState.Reciving)
                       .Using(state.StateData.SetData(accept.Target, accept.TaskCompletionSource).Open());
                    state.StateData.TargetManager.Tell(new BeginTransfering(state.StateData.OperationId));

                    return newState;
                }
                catch (Exception e)
                {
                    _log.Error(e, "Open Reciving Stream Failed {Id}", GetId(state));

                    return GoTo(OperatorState.Failed).Using(state.StateData.Failed(Parent, FailReason.StreamError, e.Message));
                }
            default:
                return null;
        }
    }

    private State<OperatorState, OperatorData>? ProcessAsInitSending(Event<OperatorData> state)
    {
        switch (state.FsmEvent)
        {
            case BeginTransfering:
                _log.Info("Start Tranfer {Id}", GetId(state));
                try
                {
                    return GoTo(OperatorState.Sending)
                       .Using(state.StateData.Open())
                       .ReplyingSelf(new StartTrensfering(state.StateData.OperationId));
                }
                catch (Exception e)
                {
                    _log.Error(e, "Open Sending Stream Failed {Id}", GetId(state));

                    return GoTo(OperatorState.Failed)
                       .Using(state.StateData.Failed(Parent, FailReason.StreamError, e.Message));
                }
            case RequestDeny:
                _log.Info("Tranfer Request Deny {Id}", state.StateData.OperationId);

                return GoTo(OperatorState.Failed)
                   .Using(state.StateData.Failed(Parent, FailReason.Deny, null));
            default:
                return null;
        }
    }

    private State<OperatorState, OperatorData>? ProcessAsWaiting(Event<OperatorData> state)
    {
        switch (state.FsmEvent)
        {
            case TransmitRequest transmit:
                _log.Info("Incoming Recieve Request  {id} -- {Data}", GetId(state), transmit.Data);

                return GoTo(OperatorState.InitReciving)
                   .ReplyingParent(new IncomingDataTransfer(transmit.OperationId, new DataTransferManager(Parent), transmit.Data))
                   .Using(state.StateData.StartRecdiving(transmit));
            case DataTransferRequest request:
                _log.Info("Incoming Trensfer Request {id} -- {Data}", GetId(state), request.Data);

                return GoTo(OperatorState.InitSending)
                   .Replying(new TransmitRequest(request.OperationId, Parent, request.Data), request.Target.Actor, Parent)
                   .Using(state.StateData.StartSending(request, Sender));
            default:
                return null;
        }
    }

    private static IActorRef Parent => Context.Parent;

    private static FileOperationId GetId(Event<OperatorData> message)
    {
        FileOperationId id = (message.FsmEvent as TransferMessage)?.OperationId ?? message.StateData.OperationId;
        if(string.IsNullOrWhiteSpace(id.Value))
            id = FileOperationId.From("Unkowen");

        return id;
    }
}