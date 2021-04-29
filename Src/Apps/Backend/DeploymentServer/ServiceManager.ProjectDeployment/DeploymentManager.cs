using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using ServiceManager.ProjectDeployment.Actors;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Features;

namespace ServiceManager.ProjectDeployment
{
    [PublicAPI]
    public sealed class DeploymentManager
    {
        public const string DeploymentManagerKey = "DeploymentManager";

        public static readonly DeploymentManager Empty = new(ActorRefs.Nobody);

        private readonly IActorRef _manager;

        private DeploymentManager(IActorRef manager) => _manager = manager;

        public bool IsOk => !_manager.IsNobody();

        public static DeploymentManager CreateInstance(IActorRefFactory factory, DeploymentConfiguration configuration)
            => new(factory.ActorOf(
                DeploymentApi.DeploymentPath, 
                DeploymentServerImpl.New(configuration.Configuration, configuration.FileSystem, configuration.Manager, configuration.RepositoryApi)));
        
        public static DeploymentManager InitDeploymentManager(ActorSystem actorSystem, DeploymentConfiguration configuration)
        {
            var repo = ClusterSingletonManager.Props(
                Feature.Props(DeploymentServerImpl.New(configuration.Configuration, configuration.FileSystem, configuration.Manager, configuration.RepositoryApi)),
                ClusterSingletonManagerSettings.Create(actorSystem).WithRole("UpdateSystem"));
            return new DeploymentManager(actorSystem.ActorOf(repo, DeploymentApi.DeploymentPath));
        }

        public void Stop()
            => _manager.Tell(PoisonPill.Instance);
    }
}