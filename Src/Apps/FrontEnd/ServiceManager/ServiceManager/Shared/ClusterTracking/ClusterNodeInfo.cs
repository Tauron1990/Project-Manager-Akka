using Akka.Cluster;
using Newtonsoft.Json;
using Tauron.Application;

namespace ServiceManager.Shared.ClusterTracking
{
    public sealed record ClusterNodeInfo(string Name, string ServiceType, string Status, string Url);
}