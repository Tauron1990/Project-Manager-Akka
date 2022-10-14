namespace Tauron.Application.AkkaNode.Services.FileTransfer;

public sealed class TransferSucess : TransferMessages.TransferCompled
{
    public TransferSucess(FileOperationId operationId, string? data)
        : base(operationId, data) { }
}