using Akka.Actor;
using Akka.DependencyInjection;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public sealed class ActorRefFactory<TActor> where TActor : ActorBase
    {
        private readonly ActorSystem _system;

        public ActorRefFactory(ActorSystem system) => _system = system;

        public IActorRef Create(bool sync, string? name = null) => _system.ActorOf(CreateProps(sync), name);

        public Props CreateProps(bool sync)
        {
            var prop = _system.GetExtension<ServiceProvider>().Props<TActor>();
            if (sync)
                prop = prop.WithDispatcher("synchronized-dispatcher");

            return prop;
        }
    }
}