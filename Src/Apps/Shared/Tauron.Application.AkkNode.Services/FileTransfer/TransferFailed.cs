namespace Tauron.Application.AkkaNode.Services.FileTransfer;

public enum FailReason
{
    Unkowen,
    DuplicateOperationId,
    CorruptState,
    Deny,
    StreamError,
    ComunicationError,
    Timeout,
    ToManyResends,
    ReadError,
    WriteError
}

public sealed class TransferFailed : TransferMessages.TransferCompled
{
    public TransferFailed(FileOperationId operationId, FailReason reason, TransferData data)
        : base(operationId, data)
        => Reason = reason;

    public FailReason Reason { get; }
}