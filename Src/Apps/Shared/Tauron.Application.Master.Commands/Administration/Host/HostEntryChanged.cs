using Akka.Actor;

namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed record HostEntryChanged(string Name, ActorPath Path, bool Removed);
}