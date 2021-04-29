using System;
using System.Reactive.Linq;
using Akka.Actor;
using ServiceManager.ProjectRepository.Data;
using SharpRepository.Repository;
using Tauron;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Files.VirtualFiles;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Features;

namespace ServiceManager.ProjectRepository.Actors
{
    internal sealed class RepositoryManagerImpl : ActorFeatureBase<RepositoryManagerImpl.RmIState>
    {
        public static IPreparedFeature Create(RepositoryManagerConfiguration configuration)
        {
            return Feature.Create(
                () => new RepositoryManagerImpl(), 
                _ => new RmIState(configuration.RepositoryConfiguration.GetInstance<RepositoryEntry, string>(RepositoryManager.RepositoryManagerKey), configuration.FileSystem, configuration.DataTransferManager));
        }


        protected override void ConfigImpl()
        {
            Receive<Status>().Subscribe();
            Receive<IRepositoryAction>(obs => obs.Select(d => 
                                                         (
                                                             Actor: Context.ActorOf(OperatorActor.New(d.State.Repositorys, d.State.Bucket, d.State.DataTransferManager)),
                                                             d.Event
                                                         ))
                                                 .ToUnit(evt => evt.Actor.Forward(evt.Event)));

            SupervisorStrategy = SupervisorStrategy.StoppingStrategy;
        }

        public sealed record RmIState(IRepository<RepositoryEntry, string> Repositorys, IDirectory Bucket, DataTransferManager DataTransferManager);

        private sealed record IndexRequest;
    }
}