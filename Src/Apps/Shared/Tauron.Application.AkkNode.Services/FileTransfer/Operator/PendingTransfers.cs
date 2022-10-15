using System;
using System.Collections.Immutable;
using Akka.Actor;

namespace Tauron.Application.AkkaNode.Services.FileTransfer.Operator;

public sealed class PendingTransfers
{
    private readonly ImmutableDictionary<FileOperationId, IncomingDataTransfer> _data;

    private PendingTransfers(ImmutableDictionary<FileOperationId, IncomingDataTransfer> data)
        => _data = data;

    internal static PendingTransfers New() => new(ImmutableDictionary<FileOperationId, IncomingDataTransfer>.Empty);

    internal PendingTransfers Remove(in FileOperationId id) => new(_data.Remove(id));

    internal PendingTransfers NewTransfer(IncomingDataTransfer transfer) => new(_data.SetItem(transfer.OperationId, transfer));

    internal PendingTransfers ProcessAwait(IActorRef sender, AwaitRequest awaitRequest, Action noPending)
    {
        if(_data.TryGetValue(awaitRequest.Id, out IncomingDataTransfer? transfer))
        {
            sender.Tell(new AwaitResponse(transfer));

            return new PendingTransfers(_data.Remove(awaitRequest.Id));
        }

        noPending();

        return this;
    }
}