using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services;
using Tauron.Application.AkkaNode.Services.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Repository
{
    [PublicAPI]
    public sealed class RepositoryApi : ISender
    {
        public const string RepositoryPath = @"RepositoryManager";

        private readonly IActorRef _repository;

        private RepositoryApi(IActorRef repository) => _repository = repository;
        public static RepositoryApi Empty { get; } = new(ActorRefs.Nobody);

        void ISender.SendCommand(IReporterMessage command) => _repository.Tell(command);

        public static RepositoryApi CreateFromActor(IActorRef manager)
            => new(manager);

        public static RepositoryApi CreateProxy(ActorSystem system, string name = "RepositoryProxy")
        {
            var proxy = ClusterSingletonProxy.Props($"/user/{RepositoryPath}",
                ClusterSingletonProxySettings.Create(system).WithRole("UpdateSystem"));
            return new RepositoryApi(system.ActorOf(proxy, name));
        }
    }
}