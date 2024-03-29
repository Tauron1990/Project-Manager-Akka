﻿using System;
using System.Reactive.Linq;
using Akka.Actor;
using ServiceManager.ProjectRepository.Data;
using SharpRepository.Repository;
using Tauron;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Deployment;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Application.VirtualFiles;
using Tauron.Features;

namespace ServiceManager.ProjectRepository.Actors
{
    internal sealed class RepositoryManagerImpl : ActorFeatureBase<RepositoryManagerImpl.RmIState>
    {
        internal static IPreparedFeature Create(RepositoryManagerConfiguration configuration)
        {
            return Feature.Create(
                () => new RepositoryManagerImpl(),
                _ => new RmIState(configuration.RepositoryConfiguration.GetInstance<RepositoryEntry, RepositoryName>(RepositoryManager.RepositoryKey), configuration.FileSystem, configuration.DataTransferManager));
        }


        protected override void ConfigImpl()
        {
            Receive<Status>().Subscribe();
            Receive<IRepositoryAction>(
                obs => obs.Select(
                        d =>
                        (
                            Actor: Context.ActorOf(OperatorActor.New(d.State.Repositorys, d.State.Bucket, d.State.DataTransferManager)),
                            d.Event
                        ))
                   .ToUnit(evt => evt.Actor.Forward(evt.Event)));

            SupervisorStrategy = SupervisorStrategy.StoppingStrategy;
        }

        public sealed record RmIState(IRepository<RepositoryEntry, RepositoryName> Repositorys, IDirectory Bucket, DataTransferManager DataTransferManager);
    }
}