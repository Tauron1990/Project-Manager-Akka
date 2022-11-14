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