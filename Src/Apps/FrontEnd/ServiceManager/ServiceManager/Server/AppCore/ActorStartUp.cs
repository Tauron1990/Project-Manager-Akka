using ServiceManager.Server.AppCore.ClusterTracking;
using ServiceManager.Server.AppCore.Helper;
using Tauron.Application.AkkaNode.Bootstrap;

namespace ServiceManager.Server.AppCore
{
    public sealed class ActorStartUp : IStartUpAction
    {
        private readonly IClusterNodeManager _manager;
        private readonly IApiEventDispatcher _dispatcher;

        public ActorStartUp(IClusterNodeManager manager, IApiEventDispatcher dispatcher)
        {
            _manager = manager;
            _dispatcher = dispatcher;
        }

        public void Run()
        {
            _manager.Tell(new InitActor());
            _dispatcher.Init();
        }
    }
}