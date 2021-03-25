using System.Collections.Immutable;
using Akka.Actor;
namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed record TargetProcessRegistration(ImmutableArray<string> FileNames, IActorRef Target);
}