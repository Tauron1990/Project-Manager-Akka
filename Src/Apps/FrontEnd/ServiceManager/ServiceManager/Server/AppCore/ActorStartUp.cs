using MongoDB.Driver.Core.Clusters;
using ServiceManager.Server.AppCore.ClusterTracking;
using Tauron.Application.AkkaNode.Bootstrap;

namespace ServiceManager.Server.AppCore
{
    public sealed class ActorStartUp : IStartUpAction
    {
        private readonly IClusterNodeManager _manager;

        public ActorStartUp(IClusterNodeManager manager) 
            => _manager = manager;

        public void Run()
        {
            _manager.Tell(new ClusterHostManagerActor.InitActor());
        }
    }
}