using System;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.AkkaNode.Services.FileTransfer;

public static class TransferMessages
{
    [PublicAPI]
    public abstract class TransferMessage
    {
        protected TransferMessage(FileOperationId operationId) => OperationId = operationId;
        public FileOperationId OperationId { get; }
    }

    public abstract class TransferCompled : TransferMessage
    {
        protected TransferCompled(FileOperationId operationId, string? data) : base(operationId) => Data = data;
        public string? Data { get; }
    }

    public abstract class DataTranfer : TransferMessage
    {
        protected DataTranfer(FileOperationId operationId) : base(operationId) { }
    }

    [PublicAPI]
    public sealed class TransferError : DataTranfer
    {
        public TransferError(FileOperationId operationId, FailReason failReason, string? data)
            : base(operationId)
        {
            FailReason = failReason;
            Data = data;
        }

        public FailReason FailReason { get; }

        public string? Data { get; }

        public TransferFailed ToFailed()
            => new(OperationId, FailReason, Data);
    }

    public sealed class NextChunk : DataTranfer
    {
        public NextChunk(FileOperationId operationId, byte[] data, int count, bool finish, uint crc, uint finishCrc) : base(
            operationId)
        {
            Data = data;
            Count = count;
            Finish = finish;
            Crc = crc;
            FinishCrc = finishCrc;
        }

        public byte[] Data { get; }

        public int Count { get; }

        public bool Finish { get; }

        public uint Crc { get; }

        public uint FinishCrc { get; }
    }

    public sealed class SendNextChunk : DataTranfer
    {
        public SendNextChunk(FileOperationId operationId) : base(operationId) { }
    }

    public sealed class SendingCompled : DataTranfer
    {
        public SendingCompled(FileOperationId operationId)
            : base(operationId) { }
    }

    public sealed class RepeadChunk : DataTranfer
    {
        public RepeadChunk(FileOperationId operationId)
            : base(operationId) { }
    }

    public sealed class StartTrensfering : DataTranfer
    {
        public StartTrensfering(FileOperationId operationId)
            : base(operationId) { }
    }

    public sealed class BeginTransfering : DataTranfer
    {
        public BeginTransfering(FileOperationId operationId)
            : base(operationId) { }
    }

    public sealed class TransmitRequest : DataTranfer
    {
        public TransmitRequest(FileOperationId operationId, IActorRef from, string? data) : base(operationId)
        {
            From = from;
            Data = data;
        }

        public IActorRef From { get; }

        public string? Data { get; }
    }

    public sealed class RequestAccept : DataTranfer
    {
        public RequestAccept(
            FileOperationId operationId, Func<ITransferData> target,
            TaskCompletionSource<TransferCompled> taskCompletionSource)
            : base(operationId)
        {
            Target = target;
            TaskCompletionSource = taskCompletionSource;
        }

        public Func<ITransferData> Target { get; }
        public TaskCompletionSource<TransferCompled> TaskCompletionSource { get; }
    }

    public sealed class RequestDeny : DataTranfer
    {
        public RequestDeny(FileOperationId operationId) : base(operationId) { }
    }
}