using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using ServiceManager.ProjectRepository.Actors;
using Tauron.Application.AkkaNode.Services.Core;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Features;

namespace ServiceManager.ProjectRepository
{
    [PublicAPI]
    public sealed class RepositoryManager
    {
        public const string RepositoryKey = "ReporitoryManager";

        public static readonly RepositoryManager Empty = new(ActorRefs.Nobody);

        public IActorRef Manager { get; }

        private RepositoryManager(IActorRef manager) => Manager = manager;

        public bool IsOk => !Manager.IsNobody();

        public static RepositoryManager CreateInstance(IActorRefFactory factory, RepositoryManagerConfiguration configuration)
            => new(factory.ActorOf(RepositoryApi.RepositoryPath, RepositoryManagerImpl.Create(configuration), TellAliveFeature.New()));


        public static RepositoryManager InitRepositoryManager(ActorSystem actorSystem, RepositoryManagerConfiguration configuration)
        {
            var repo = ClusterSingletonManager.Props(
                Feature.Props(RepositoryManagerImpl.Create(configuration), TellAliveFeature.New()),
                ClusterSingletonManagerSettings.Create(actorSystem).WithRole("UpdateSystem"));
            
            return new RepositoryManager(actorSystem.ActorOf(repo, RepositoryApi.RepositoryPath));
        }

        public void Stop()
            => Manager.Tell(PoisonPill.Instance);
    }
}