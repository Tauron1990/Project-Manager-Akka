using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using ServiceManager.ProjectRepository.Actors;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Features;

namespace ServiceManager.ProjectRepository
{
    [PublicAPI]
    public sealed class RepositoryManager
    {
        public const string RepositoryManagerKey = "ReporitoryManager";

        public static readonly RepositoryManager Empty = new(ActorRefs.Nobody);

        private readonly IActorRef _manager;

        private RepositoryManager(IActorRef manager) => _manager = manager;

        public bool IsOk => !_manager.IsNobody();

        public static RepositoryManager CreateInstance(IActorRefFactory factory, RepositoryManagerConfiguration configuration)
            => new(factory.ActorOf(RepositoryManagerImpl.Create(configuration)));


        public static RepositoryManager InitRepositoryManager(ActorSystem actorSystem, RepositoryManagerConfiguration configuration)
        {
            var repo = ClusterSingletonManager.Props(
                Feature.Props(RepositoryManagerImpl.Create(configuration)),
                ClusterSingletonManagerSettings.Create(actorSystem).WithRole("UpdateSystem"));
            
            return new RepositoryManager(actorSystem.ActorOf(repo, RepositoryApi.RepositoryPath));
        }

        public void Run(IRepositoryAction action)
            => _manager.Tell(action);

        public void Stop()
            => _manager.Tell(PoisonPill.Instance);
    }
}