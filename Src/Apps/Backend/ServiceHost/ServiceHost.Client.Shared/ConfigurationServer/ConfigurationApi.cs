using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;

namespace ServiceHost.Client.Shared.ConfigurationServer
{
    [PublicAPI]
    public sealed class ConfigurationApi : ISender
    {
        public const string ConfigurationPath = "/ServiceDeamon/ConfigurationManager";

        private readonly IActorRef _api;

        private ConfigurationApi(IActorRef api) => _api = api;

        void ISender.SendCommand(IReporterMessage command) => _api.Tell(command);


        public static ConfigurationApi CreateFromActor(IActorRef actor)
            => new(actor);

        public static ConfigurationApi CreateProxy(ActorSystem system, string name = "DeploymentProxy")
        {
            var proxy = ClusterSingletonProxy.Props($"/user/{ConfigurationPath}",
                ClusterSingletonProxySettings.Create(system).WithRole("Service-Manager"));
            return new ConfigurationApi(system.ActorOf(proxy, name));
        }
    }
}