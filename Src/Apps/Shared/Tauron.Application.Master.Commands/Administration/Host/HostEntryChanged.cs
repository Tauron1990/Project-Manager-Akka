using Akka.Actor;

namespace Tauron.Application.Master.Commands.Administration.Host;

public sealed record HostEntryChanged(HostName Name, ActorPath Path, bool Removed);