using System;
using System.Collections.Immutable;
using Akka.Actor;

namespace Tauron.Application.AkkaNode.Services.FileTransfer.Operator;

public sealed class Awaiters
{
    private readonly ImmutableDictionary<FileOperationId, AwaitRequestInternal> _awaiters;
    
    private Awaiters(ImmutableDictionary<FileOperationId, AwaitRequestInternal> awaiters)
        => _awaiters = awaiters;

    internal static Awaiters New() => new(ImmutableDictionary<FileOperationId, AwaitRequestInternal>.Empty);

    internal Awaiters ProcessWaiter(IncomingDataTransfer transfer, Action noTransfer)
    {
        if(_awaiters.TryGetValue(transfer.OperationId, out AwaitRequestInternal? request))
        {
            request.Target.Tell(new AwaitResponse(transfer));
            
            return new Awaiters(_awaiters.Remove(transfer.OperationId));
        }

        noTransfer();

        return this;
    }

    public Awaiters NewWaiter(in FileOperationId id, IActorRef sender) => new(_awaiters.SetItem(id, new AwaitRequestInternal(sender)));

    public Awaiters Delete(in FileOperationId id) => new(_awaiters.Remove(id));
}