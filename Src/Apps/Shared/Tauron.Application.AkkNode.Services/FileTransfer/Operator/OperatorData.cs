using System;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using Tauron.Application.AkkaNode.Services.FileTransfer.Internal;

namespace Tauron.Application.AkkaNode.Services.FileTransfer.Operator;

public sealed record OperatorData(
    FileOperationId OperationId,
    IActorRef TargetManager,
    Func<ITransferData> Data,
    TransferData? Metadata,
    InternalCrcStream TransferStrem,
    TransferMessages.TransferError? Error,
    TaskCompletionSource<TransferMessages.TransferCompled>? Completion,
    bool SendBack,
    IActorRef Sender,
    object? IndividualMessage)
{
    public static readonly Crc32 Crc32 = new();

    public OperatorData()
        : this(
            FileOperationId.Empty,
            ActorRefs.Nobody,
            () => new StreamData(Stream.Null),
            Metadata: null,
            new InternalCrcStream(StreamData.Null),
            Error: null,
            Completion: null,
            SendBack: false,
            ActorRefs.Nobody,
            IndividualMessage: null) { }

    public OperatorData StartSending(DataTransferRequest id, IActorRef sender)
        => this with
           {
               OperationId = id.OperationId,
               TargetManager = id.Target.Actor,
               Data = id.Source,
               Metadata = id.Data,
               SendBack = id.SendCompletionBack,
               Sender = sender,
               IndividualMessage = id.IndividualMessage,
           };

    public OperatorData StartRecdiving(TransferMessages.TransmitRequest id)
        => this with { OperationId = id.OperationId, TargetManager = id.From, Metadata = id.Data };

    public OperatorData SetData(
        Func<ITransferData> data,
        TaskCompletionSource<TransferMessages.TransferCompled> completion)
        => this with { Data = data, Completion = completion };

    public OperatorData Open() => this with { TransferStrem = new InternalCrcStream(Data()) };

    public OperatorData Failed(IActorRef parent, FailReason reason, string? errorData)
    {
        var failed = new TransferMessages.TransferError(
            OperationId,
            reason,
            string.IsNullOrWhiteSpace(errorData)
                ? TransferData.Empty
                : TransferData.From(errorData));

        TargetManager.Tell(failed, parent);
        parent.Tell(failed.ToFailed());

        return this with { Error = failed };
    }

    public OperatorData InComingError(TransferMessages.TransferError error)
        => this with { Error = error };
}