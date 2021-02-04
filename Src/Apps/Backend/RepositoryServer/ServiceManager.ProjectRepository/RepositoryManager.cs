using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using MongoDB.Driver;
using ServiceManager.ProjectRepository.Actors;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Features;

namespace ServiceManager.ProjectRepository
{
    [PublicAPI]
    public sealed class RepositoryManager
    {
        public static readonly RepositoryManager Empty = new(ActorRefs.Nobody); 

        public static RepositoryManager CreateInstance(IActorRefFactory factory, string connectionString, DataTransferManager tranferManager)
        {
            return new(factory.ActorOf(RepositoryManagerImpl.Create(new MongoClient(connectionString), tranferManager)));
        }

        public static RepositoryManager InitRepositoryManager(ActorSystem actorSystem, string connectionString, DataTransferManager tranferManager) 
            => InitRepositoryManager(actorSystem, new MongoClient(connectionString), tranferManager);

        public static RepositoryManager InitRepositoryManager(ActorSystem actorSystem, IMongoClient client, DataTransferManager tranferManager)
        {
            var repo = ClusterSingletonManager.Props(Feature.Props(RepositoryManagerImpl.Create(client, tranferManager)),
                ClusterSingletonManagerSettings.Create(actorSystem).WithRole("UpdateSystem"));
            return new RepositoryManager(actorSystem.ActorOf(repo, RepositoryApi.RepositoryPath));
        }

        private readonly IActorRef _manager;

        private RepositoryManager(IActorRef manager) => _manager = manager;

        public bool IsOk => !_manager.IsNobody();

        public void CleanUp()
            => _manager.Tell(new StartCleanUp());

        public void Stop()
            => _manager.Tell(PoisonPill.Instance);
    }
}