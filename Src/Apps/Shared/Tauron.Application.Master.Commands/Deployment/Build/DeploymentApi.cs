using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.Core;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Build
{
    [PublicAPI]
    public sealed class DeploymentApi : SenderBase<DeploymentApi, IDeploymentQuery, IDeploymentCommand>, IQueryIsAliveSupport
    {
        public const string DeploymentPath = @"DeploymentManager";

        private readonly IActorRef _repository;

        private DeploymentApi(IActorRef repository) => _repository = repository;

        public Task<IsAliveResponse> QueryIsAlive(ActorSystem system, TimeSpan timeout)
            => AkkaNode.Services.Core.QueryIsAlive.Ask(system, _repository, timeout);

        protected override void SendCommandImpl(IReporterMessage command)
            => _repository.Tell(command);

        public static DeploymentApi CreateFromActor(IActorRef actor)
            => new(actor);

        public static DeploymentApi CreateProxy(ActorSystem system, string name = "DeploymentProxy")
        {
            var proxy = ClusterSingletonProxy.Props(
                $"/user/{DeploymentPath}",
                ClusterSingletonProxySettings.Create(system).WithRole("UpdateSystem"));

            return new DeploymentApi(system.ActorOf(proxy, name));
        }
    }
}