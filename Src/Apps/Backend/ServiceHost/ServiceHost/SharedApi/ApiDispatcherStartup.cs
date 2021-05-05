using Akka.Actor;
using Akka.DependencyInjection;
using Tauron.Application.AkkaNode.Bootstrap;

namespace ServiceHost.SharedApi
{
    public sealed class ApiDispatcherStartup : IStartUpAction
    {
        private readonly ActorSystem _system;

        public ApiDispatcherStartup(ActorSystem system) 
            => _system = system;

        public void Run()
        {
            var props = ServiceProvider.For(_system).Props<ApiDispatcherActor>();
            _system.ActorOf(props, "Api-Dispatcher");
        }
    }
}