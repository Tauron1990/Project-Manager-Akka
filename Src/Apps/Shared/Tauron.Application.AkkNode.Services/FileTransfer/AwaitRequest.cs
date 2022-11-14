using UnitsNet;

namespace Tauron.Application.AkkaNode.Services.FileTransfer;

public sealed class AwaitRequest
{
    public AwaitRequest(Duration? timeout, FileOperationId id)
    {
        Timeout = timeout;
        Id = id;
    }

    public Duration? Timeout { get; }

    public FileOperationId Id { get; }
}