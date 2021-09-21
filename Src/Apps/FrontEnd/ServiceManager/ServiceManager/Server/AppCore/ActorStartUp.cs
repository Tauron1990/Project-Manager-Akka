using System.Collections.Generic;
using ServiceManager.Server.AppCore.ClusterTracking;
using ServiceManager.Server.AppCore.Helper;
using Tauron;
using Tauron.Application.AkkaNode.Bootstrap;

namespace ServiceManager.Server.AppCore
{
    public sealed class ActorStartUp : IStartUpAction
    {
        private readonly IEnumerable<IEventDispatcher> _dispatcher;
        private readonly IClusterNodeManager _manager;

        public ActorStartUp(IClusterNodeManager manager, IEnumerable<IEventDispatcher> dispatcher)
        {
            _manager = manager;
            _dispatcher = dispatcher;
        }

        public void Run()
        {
            _manager.Tell(new InitActor());
            _dispatcher.Foreach(d => d.Init());
        }
    }
}