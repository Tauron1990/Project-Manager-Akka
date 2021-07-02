using Akka.Cluster;
using Tauron.Application.Master.Commands.ServiceRegistry;

namespace ServiceManager.Shared.ClusterTracking
{
    public sealed record MemberData(Member Member, string Name, ServiceType ServiceType);
}