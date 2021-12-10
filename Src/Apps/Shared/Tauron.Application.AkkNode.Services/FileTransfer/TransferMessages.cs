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
        protected TransferMessage(string operationId) => OperationId = operationId;
        public string OperationId { get; }
    }

    public abstract class TransferCompled : TransferMessage
    {
        protected TransferCompled(string operationId, string? data) : base(operationId) => Data = data;
        public string? Data { get; }
    }

    public abstract class DataTranfer : TransferMessage
    {
        protected DataTranfer(string operationId) : base(operationId) { }
    }

    [PublicAPI]
    public sealed class TransferError : DataTranfer
    {
        public TransferError(string operationId, FailReason failReason, string? data)
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
        public NextChunk(string operationId, byte[] data, int count, bool finish, uint crc, uint finishCrc) : base(
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
        public SendNextChunk(string operationId) : base(operationId) { }
    }

    public sealed class SendingCompled : DataTranfer
    {
        public SendingCompled(string operationId)
            : base(operationId) { }
    }

    public sealed class RepeadChunk : DataTranfer
    {
        public RepeadChunk(string operationId)
            : base(operationId) { }
    }

    public sealed class StartTrensfering : DataTranfer
    {
        public StartTrensfering(string operationId)
            : base(operationId) { }
    }

    public sealed class BeginTransfering : DataTranfer
    {
        public BeginTransfering(string operationId)
            : base(operationId) { }
    }

    public sealed class TransmitRequest : DataTranfer
    {
        public TransmitRequest(string operationId, IActorRef from, string? data) : base(operationId)
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
            string operationId, Func<ITransferData> target,
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
        public RequestDeny(string operationId) : base(operationId) { }
    }
}