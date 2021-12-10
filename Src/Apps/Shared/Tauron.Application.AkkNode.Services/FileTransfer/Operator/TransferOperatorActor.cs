using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.FileTransfer.Internal;
using static Tauron.Application.AkkaNode.Services.FileTransfer.TransferMessages;

namespace Tauron.Application.AkkaNode.Services.FileTransfer.Operator;

public enum OperatorState
{
    Waiting,
    InitSending,
    InitReciving,
    Sending,
    Reciving,
    Failed,
    Compled
}

public sealed class OperatorData
{
    public static readonly Crc32 Crc32 = new();

    private OperatorData(
        string operationId, IActorRef targetManager, Func<ITransferData> data, string? metadata, InternalCrcStream transferStrem, TransferError? error,
        TaskCompletionSource<TransferCompled>? completion, bool sendBack, IActorRef sender, object? individualMessgae)
    {
        OperationId = operationId;
        TargetManager = targetManager;
        Data = data;
        Metadata = metadata;
        Error = error;
        Completion = completion;
        SendBack = sendBack;
        Sender = sender;

        TransferStrem = transferStrem;
        IndividualMessage = individualMessgae;
    }

    public OperatorData()
        : this(
            string.Empty,
            ActorRefs.Nobody,
            () => new StreamData(Stream.Null),
            metadata: null,
            new InternalCrcStream(StreamData.Null),
            error: null,
            completion: null,
            sendBack: false,
            ActorRefs.Nobody,
            individualMessgae: null) { }

    public string OperationId { get; }

    public IActorRef TargetManager { get; }

    private Func<ITransferData> Data { get; }

    public string? Metadata { get; }

    public InternalCrcStream TransferStrem { get; }

    public TransferError? Error { get; }

    public TaskCompletionSource<TransferCompled>? Completion { get; }

    public bool SendBack { get; }

    public IActorRef Sender { get; }

    public object? IndividualMessage { get; }

    private OperatorData Copy(
        string? id = null, IActorRef? target = null, Func<ITransferData>? data = null, string? metadata = null, InternalCrcStream? stream = null,
        TransferError? failed = null, TaskCompletionSource<TransferCompled>? completion = null, bool? sendBack = false, IActorRef? sender = null,
        object? individualMessage = null)
        => new(
            id ?? OperationId,
            target ?? TargetManager,
            data ?? Data,
            metadata ?? Metadata,
            stream ?? TransferStrem,
            failed ?? Error,
            completion ?? Completion,
            sendBack ?? SendBack,
            sender ?? Sender,
            individualMessage ?? individualMessage);

    public OperatorData StartSending(DataTransferRequest id, IActorRef sender)
        => Copy(id.OperationId, id.Target.Actor, id.Source, id.Data, sendBack: id.SendCompletionBack, sender: sender, individualMessage: id.IndividualMessage);

    public OperatorData StartRecdiving(TransmitRequest id)
        => Copy(id.OperationId, id.From, metadata: id.Data);

    public OperatorData SetData(
        Func<ITransferData> data,
        TaskCompletionSource<TransferCompled> completion)
        => Copy(data: data, completion: completion);

    public OperatorData Open() => Copy(stream: new InternalCrcStream(Data()));

    public OperatorData Failed(IActorRef parent, FailReason reason, string? errorData)
    {
        var failed = new TransferError(OperationId, reason, errorData);
        TargetManager.Tell(failed, parent);
        parent.Tell(failed.ToFailed());

        return Copy(failed: failed);
    }

    public OperatorData InComingError(TransferError error)
        => Copy(failed: error);
}

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

        When(
            OperatorState.Waiting,
            state =>
            {
                switch (state.FsmEvent)
                {
                    case TransmitRequest transmit:
                        _log.Info("Incoming Recieve Request  {id} -- {Data}", GetId(state), transmit.Data);
                        Parent.Tell(new IncomingDataTransfer(transmit.OperationId, new DataTransferManager(Parent), transmit.Data));

                        return GoTo(OperatorState.InitReciving).Using(state.StateData.StartRecdiving(transmit));
                    case DataTransferRequest request:
                        _log.Info("Incoming Trensfer Request {id} -- {Data}", GetId(state), request.Data);
                        request.Target.Actor.Tell(new TransmitRequest(request.OperationId, Parent, request.Data), Parent);

                        return GoTo(OperatorState.InitSending).Using(state.StateData.StartSending(request, Sender));
                    default:
                        return null;
                }
            });

        When(
            OperatorState.InitSending,
            state =>
            {
                switch (state.FsmEvent)
                {
                    case BeginTransfering:
                        _log.Info("Start Tranfer {Id}", GetId(state));
                        try
                        {
                            var newState = GoTo(OperatorState.Sending).Using(state.StateData.Open());
                            Self.Tell(new StartTrensfering(state.StateData.OperationId));

                            return newState;
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
            },
            StartTimeout);

        When(
            OperatorState.InitReciving,
            state =>
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
                            var newState = GoTo(OperatorState.Reciving).Using(state.StateData.SetData(accept.Target, accept.TaskCompletionSource).Open());
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
            },
            InitTimeout);

        When(
            OperatorState.Sending,
            state =>
            {
                switch (state.FsmEvent)
                {
                    case SendNextChunk:
                    case StartTrensfering:
                        _outgoningBytes ??= ArrayPool<byte>.Shared.Rent(1024 * 1024);
                        try
                        {
                            _sendingAttempts = 0;
                            var count = state.StateData.TransferStrem.Read(_outgoningBytes, 0, _outgoningBytes.Length);
                            var last = count == 0;
                            var crc = OperatorData.Crc32.ComputeChecksum(_outgoningBytes, count);

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
                    case SendingCompled:
                        state.StateData.TransferStrem.Dispose();
                        if (_outgoningBytes != null)
                            ArrayPool<byte>.Shared.Return(_outgoningBytes);
                        _outgoningBytes = null;

                        var comp = new TransferSucess(state.StateData.OperationId, state.StateData.Metadata);
                        Parent.Tell(comp);
                        if (state.StateData.SendBack)
                            state.StateData.Sender.Tell(comp);
                        if (state.StateData.IndividualMessage != null)
                            state.StateData.Sender.Tell(state.StateData.IndividualMessage);

                        return GoTo(OperatorState.Compled);
                    case RepeadChunk:
                        _sendingAttempts += 1;

                        if (_sendingAttempts > 5)
                            return GoTo(OperatorState.Failed)
                               .Using(state.StateData.Failed(Parent, FailReason.ToManyResends, null));

                        state.StateData.TargetManager.Tell(_lastChunk, Parent);

                        return Stay();
                    default:
                        return null;
                }
            },
            SendingTimeout);

        When(
            OperatorState.Reciving,
            state =>
            {
                switch (state.FsmEvent)
                {
                    case NextChunk chunk:
                        try
                        {
                            var reciveCrc = OperatorData.Crc32.ComputeChecksum(chunk.Data, chunk.Count);
                            if (reciveCrc != chunk.Crc)
                            {
                                state.StateData.TargetManager.Tell(new RepeadChunk(state.StateData.OperationId), Parent);
                            }
                            else
                            {
                                if (chunk.Count > 0)
                                    state.StateData.TransferStrem.Write(chunk.Data, 0, chunk.Count);

                                if (chunk.Finish)
                                {
                                    var data = state.StateData;
                                    try
                                    {
                                        if (data.TransferStrem.WriteCrc != chunk.FinishCrc)
                                            return GoTo(OperatorState.Failed)
                                               .Using(state.StateData.Failed(Parent, FailReason.ComunicationError, null));

                                        data.TargetManager.Tell(new SendingCompled(state.StateData.OperationId));

                                        var msg = new TransferSucess(state.StateData.OperationId, state.StateData.Metadata);
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
                            }

                            return Stay();
                        }
                        catch (Exception e)
                        {
                            _log.Error(e, "Error on Write Stream");

                            return GoTo(OperatorState.Failed)
                               .Using(state.StateData.Failed(Parent, FailReason.WriteError, e.Message));
                        }
                    case RequestDeny:
                        return Stay();
                    default:
                        return null;
                }
            },
            SendingTimeout);


        When(
            OperatorState.Failed,
            state =>
            {
                _log.Warning("Transfer Failed {Id}", GetId(state));

                void Set(TransferCompled failed)
                {
                    state.StateData.Completion?.SetResult(failed);
                    if (state.StateData.SendBack)
                        state.StateData.Sender.Tell(failed);
                    if (state.StateData.IndividualMessage != null)
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
                        Set(new TransferFailed(GetId(state), FailReason.Timeout, null));

                        break;
                    default:
                    {
                        var manmesg = state.StateData.Error ??
                                      new TransferError(
                                          (state.FsmEvent as TransferMessage)?.OperationId ??
                                          state.StateData.OperationId,
                                          FailReason.CorruptState,
                                          null);

                        state.StateData.TargetManager.Tell(manmesg, Parent);
                        Set(manmesg.ToFailed());

                        break;
                    }
                }

                return Stay();
            },
            FailedTimeout);

        When(OperatorState.Compled, _ => null);

        OnTransition(
            (_, nextState) =>
            {
                if (nextState != OperatorState.Failed && nextState != OperatorState.Compled) return;

                NextStateData.TransferStrem.Dispose();
                if (_outgoningBytes != null)
                    ArrayPool<byte>.Shared.Return(_outgoningBytes);
            });

        WhenUnhandled(
            state =>
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
            });

        Initialize();
    }

    private static IActorRef Parent => Context.Parent;

    private static string GetId(Event<OperatorData> message)
    {
        var id = (message.FsmEvent as TransferMessage)?.OperationId ?? message.StateData.OperationId;
        if (string.IsNullOrWhiteSpace(id))
            id = "Unkowen";

        return id;
    }
}