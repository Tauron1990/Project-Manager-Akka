namespace Tauron.Application.AkkaNode.Services.FileTransfer;

public sealed class TransferSucess : TransferMessages.TransferCompled
{
    public TransferSucess(FileOperationId operationId, TransferData data)
        : base(operationId, data) { }
}