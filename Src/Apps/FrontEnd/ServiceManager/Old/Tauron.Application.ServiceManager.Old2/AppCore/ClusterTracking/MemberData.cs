using Akka.Cluster;
using Tauron.Application.Master.Commands.ServiceRegistry;

namespace Tauron.Application.ServiceManager.AppCore.ClusterTracking
{
    public sealed record MemberData(Member Member, string Name, ServiceType ServiceType);
}