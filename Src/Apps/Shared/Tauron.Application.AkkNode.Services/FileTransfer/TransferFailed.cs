namespace Tauron.Application.AkkaNode.Services.FileTransfer;

public sealed class TransferFailed : TransferMessages.TransferCompled
{
    public TransferFailed(FileOperationId operationId, FailReason reason, TransferData data)
        : base(operationId, data)
        => Reason = reason;

    public FailReason Reason { get; }
}