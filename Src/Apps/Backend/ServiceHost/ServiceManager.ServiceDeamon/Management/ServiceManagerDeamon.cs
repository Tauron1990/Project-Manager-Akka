using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using ServiceManager.ServiceDeamon.ConfigurationServer;
using SharpRepository.Repository.Configuration;
using Tauron.Features;

namespace ServiceManager.ServiceDeamon.Management
{
    public sealed class ServiceManagerDeamon : ActorFeatureBase<ServiceManagerDeamon.ServiceManagerDeamonState>
    {
        public const string RepositoryKey = "ServiceDeamon";
        public const string RoleName = "Service-Manager";

        public sealed record ServiceManagerDeamonState(ISharpRepositoryConfiguration Repository);

        public static void Start(ActorSystem system, ISharpRepositoryConfiguration repository) 
            => system.ActorOf("ServiceDeamon", Feature.Create(() => new ServiceManagerDeamon(), _ => new ServiceManagerDeamonState(repository)));


        protected override void ConfigImpl()
        {
            Context.ActorOf(ClusterSingletonManager.Props(ConfigurationManagerActor.New(CurrentState.Repository),
                ClusterSingletonManagerSettings.Create(Context.System)
                                               .WithRole(RoleName)), "ConfigurationManager");

            SupervisorStrategy = SupervisorStrategy.DefaultStrategy;
        }
    }
}