using Akka.Actor;

namespace Tauron.Application.AkkaNode.Services.FileTransfer.Operator;

internal sealed record AwaitRequestInternal(IActorRef Target);