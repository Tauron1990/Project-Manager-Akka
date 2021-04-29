using System;
using Akka.Actor;
using Akka.Routing;
using ServiceManager.ProjectDeployment.Build;
using ServiceManager.ProjectDeployment.Data;
using SharpRepository.Repository.Configuration;
using Tauron;
using Tauron.Application.AkkaNode.Services.CleanUp;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Files.VirtualFiles;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Application.Workshop;
using Tauron.Features;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class DeploymentServerImpl : ActorFeatureBase<DeploymentServerImpl.DeploymentServerState>
    {
        public sealed record DeploymentServerState(IActorRef Query, IActorRef Processor);

        private static DeploymentServerState CreateState(IActorRefFactory context, ISharpRepositoryConfiguration configuration, IVirtualFileSystem fileSystem, DataTransferManager manager,
            RepositoryApi repositoryApi)
        {
            var changeTracker = context.ActorOf("Change-Tracker", ChangeTrackerActor.New());
            
            var trashBin = configuration.GetInstance<ToDeleteRevision, string>("TrashBin");

            var cleanUp = context.ActorOf("CleanUp-Manager", CleanUpManager.New(configuration, fileSystem));
            cleanUp.Tell(CleanUpManager.Initialization);

            var router = new SmallestMailboxPool(Environment.ProcessorCount)
               .WithSupervisorStrategy(SupervisorStrategy.DefaultStrategy);

            var queryProps = Feature.Props(AppQueryHandler.New(configuration.GetInstance<AppData, string>(), fileSystem, manager, changeTracker))
                                  .WithRouter(router);
            var query = context.ActorOf(queryProps, "QueryRouter");

            var buildSystem = WorkDistributorFeature<BuildRequest, BuildCompled>
               .Create(context, Props.Create(() => new BuildingActor(manager)), "Compiler", TimeSpan.FromHours(1), "CompilerSupervisor");

            var processorProps = Feature.Props(AppCommandProcessor.New(configuration.GetInstance<AppData, string>(), fileSystem, repositoryApi, manager, trashBin, buildSystem, changeTracker))
                                      .WithRouter(router);
            var processor = context.ActorOf(processorProps, "ProcessorRouter");

            return new DeploymentServerState(query, processor);
        }

        public static IPreparedFeature New(ISharpRepositoryConfiguration configuration, IVirtualFileSystem fileSystem, DataTransferManager manager, RepositoryApi repositoryApi)
            => Feature.Create(() => new DeploymentServerImpl(), c => CreateState(c, configuration, fileSystem, manager, repositoryApi));

        protected override void ConfigImpl()
        {
            Receive<IDeploymentQuery>(obs => obs.ForwardToActor(i => i.State.Query, i => i.Event));
            Receive<IDeploymentCommand>(obs => obs.ForwardToActor(i => i.State.Processor, i => i.Event));
        }
    }
}