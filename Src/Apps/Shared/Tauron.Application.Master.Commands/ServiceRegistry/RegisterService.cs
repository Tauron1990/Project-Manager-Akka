using Akka.Cluster;

namespace Tauron.Application.Master.Commands
{
    public sealed record RegisterService(string Name, UniqueAddress Address);
}