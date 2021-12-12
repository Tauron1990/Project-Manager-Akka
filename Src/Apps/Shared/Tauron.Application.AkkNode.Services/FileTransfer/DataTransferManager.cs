using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.Core;
using Tauron.Application.AkkaNode.Services.FileTransfer.Operator;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Features;

namespace Tauron.Application.AkkaNode.Services.FileTransfer;

[PublicAPI]
public sealed class DataTransferManager
{
    public static DataTransferManager Empty => new(ActorRefs.Nobody);
    
    public DataTransferManager(IActorRef actor) => Actor = actor;

    public IActorRef Actor { get; }

    public static DataTransferManager New(IActorRefFactory factory, string? name = null)
        => new(factory.ActorOf(name, DataTransferManagerFeature.New(), SubscribeFeature.New()));

    public EventSubscribtion Event<TType>()
        where TType : TransferMessages.TransferMessage
        => Actor.SubscribeToEvent<TType>();

    public void Request(DataTransferRequest request)
        => Actor.Tell(request);

    public FileTransactionId RequestWithTransaction(DataTransferRequest request)
    {
        Actor.Tell(request);

        return new FileTransactionId(request.OperationId);
    }

    public Task<AwaitResponse> AskAwaitOperation(AwaitRequest request)
        => Actor.Ask<AwaitResponse>(
            request,
            request.Timeout == Timeout.InfiniteTimeSpan
                ? request.Timeout
                : request.Timeout + TimeSpan.FromSeconds(2));

    public void AwaitOperation(AwaitRequest request)
        => Actor.Tell(request);
}