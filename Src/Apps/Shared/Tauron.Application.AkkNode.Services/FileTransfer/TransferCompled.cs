namespace Tauron.Application.AkkaNode.Services.FileTransfer
{
    public sealed class TransferCompled : TransferMessages.TransferCompled
    {
        public TransferCompled(string operationId, string? data) 
            : base(operationId, data)
        {
        }
    }
}