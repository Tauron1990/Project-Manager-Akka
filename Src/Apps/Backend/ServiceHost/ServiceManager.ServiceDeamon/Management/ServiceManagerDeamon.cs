using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using ServiceManager.ServiceDeamon.ConfigurationServer;
using SharpRepository.Repository.Configuration;
using Tauron.Application.AkkaNode.Services.Core;
using Tauron.Features;

namespace ServiceManager.ServiceDeamon.Management
{
    public sealed class ServiceManagerDeamon : ActorFeatureBase<ServiceManagerDeamon.ServiceManagerDeamonState>
    {
        public const string RepositoryKey = "ServiceDeamon";
        public const string RoleName = "Service-Manager";

        public static IActorRef Init(ActorSystem system, ISharpRepositoryConfiguration repository)
            => system.ActorOf("ServiceDeamon", Feature.Create(() => new ServiceManagerDeamon(), _ => new ServiceManagerDeamonState(repository)), TellAliveFeature.New());


        protected override void ConfigImpl()
        {
            Context.ActorOf(
                ClusterSingletonManager.Props(
                    ConfigurationManagerActor.New(CurrentState.Repository),
                    ClusterSingletonManagerSettings.Create(Context.System)
                       .WithRole(RoleName)),
                "ConfigurationManager");

            SupervisorStrategy = SupervisorStrategy.DefaultStrategy;
        }

        public sealed record ServiceManagerDeamonState(ISharpRepositoryConfiguration Repository);
    }
}