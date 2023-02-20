using System.Collections.Generic;
using ServiceManager.Server.AppCore.ClusterTracking;
using ServiceManager.Server.AppCore.Helper;
using Tauron;

namespace ServiceManager.Server.AppCore
{
    public sealed class ActorStartUp
    {
        private readonly IEnumerable<IEventDispatcher> _dispatcher;
        private readonly ClusterNodeManagerRef _manager;

        public ActorStartUp(ClusterNodeManagerRef manager, IEnumerable<IEventDispatcher> dispatcher)
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