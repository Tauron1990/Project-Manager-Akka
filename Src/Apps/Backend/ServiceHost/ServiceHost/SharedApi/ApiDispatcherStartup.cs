using Akka.Actor;
using Akka.DependencyInjection;

namespace ServiceHost.SharedApi
{
    public sealed class ApiDispatcherStartup
    {
        private readonly ActorSystem _system;

        public ApiDispatcherStartup(ActorSystem system)
            => _system = system;

        public void Run()
        {
            var props = DependencyResolver.For(_system).Props<ApiDispatcherActor>();
            _system.ActorOf(props, "Api-Dispatcher");
        }
    }
}