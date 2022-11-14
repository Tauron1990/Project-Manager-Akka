using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.AkkaNode.Services.FileTransfer;

[PublicAPI]
public sealed class IncomingDataTransfer : TransferMessages.TransferMessage
{
    private readonly Timer _denyTimer;

    public IncomingDataTransfer(FileOperationId operationId, DataTransferManager manager, TransferData data)
        : base(operationId)
    {
        Manager = manager;
        Data = data;

        _denyTimer = new Timer(_ => Deny(), state: null, TimeSpan.FromMinutes(1), Timeout.InfiniteTimeSpan);
    }

    public DataTransferManager Manager { get; }

    public TransferData Data { get; }

    public event EventHandler? DenyEvent;

    public void Deny()
    {
        DenyEvent?.Invoke(this, EventArgs.Empty);
        _denyTimer.Dispose();
        Manager.Actor.Tell(new TransferMessages.RequestDeny(OperationId));
    }

    public Task<TransferMessages.TransferCompled> Accept(Func<Stream> to) => Accept(() => new StreamData(to()));

    public Task<TransferMessages.TransferCompled> Accept(Func<ITransferData> to)
    {
        _denyTimer.Dispose();
        var source = new TaskCompletionSource<TransferMessages.TransferCompled>();
        Manager.Actor.Tell(new TransferMessages.RequestAccept(OperationId, to, source));

        return source.Task;
    }
}