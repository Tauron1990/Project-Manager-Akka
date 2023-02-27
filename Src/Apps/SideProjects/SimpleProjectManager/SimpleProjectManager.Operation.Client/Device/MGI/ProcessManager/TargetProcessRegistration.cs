using System.Collections.Immutable;
using Akka.Actor;

namespace SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager
{
    public sealed record TargetProcessRegistration(ImmutableArray<string> FileNames, IActorRef Target);
}