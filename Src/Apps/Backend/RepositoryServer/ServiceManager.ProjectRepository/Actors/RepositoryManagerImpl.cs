using System;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Event;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectRepository.Data;
using Tauron;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.CleanUp;
using Tauron.Application.AkkNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Features;
using Tauron.ObservableExt;
using ActorRefFactoryExtensions = Tauron.Akka.ActorRefFactoryExtensions;

namespace ServiceManager.ProjectRepository.Actors
{
    internal sealed class RepositoryManagerImpl : ActorFeatureBase<RepositoryManagerImpl.RmIState>
    {
        public sealed record RmIState(IMongoDatabase Database, IMongoCollection<ToDeleteRevision> TrashBin, GridFSBucket GridFsBucket, DataTransferManager DataTransferManager,
            IMongoCollection<RepositoryEntry> RepositoryData);

        public static IPreparedFeature Create(IMongoClient client, DataTransferManager dataTransferManager)
        {
            var database = client.GetDatabase("Repository");

            IMongoCollection<RepositoryEntry> RepositoryData() => database.GetCollection<RepositoryEntry>("Repositorys");
            IMongoCollection<ToDeleteRevision> TrashBin() => database.GetCollection<ToDeleteRevision>("TrashBin");
            GridFSBucket GridFsBucket() => new(database, new GridFSBucketOptions {BucketName = "RepositoryData", ChunkSizeBytes = 1048576});

            return Feature.Create(() => new RepositoryManagerImpl(), () => new RmIState(database, TrashBin(), GridFsBucket(), dataTransferManager, RepositoryData()));
        }


        protected override void Config()
        {
            IActorRef CreateCleaner()
            {
                var cleaner = Context.ActorOf("CleanUp-Manager", CleanUpManager.New(CurrentState.Database, "CleanUp", CurrentState.TrashBin, CurrentState.GridFsBucket));
                cleaner.Tell(CleanUpManager.Initialization);
                Context.Watch(cleaner);
                return cleaner;
            }

            var cleaner = CreateCleaner().ToRx();
            Receive<Terminated>(obs => obs.Where(t => t.Event.ActorRef.Equals(cleaner.Value))
                                          .ToUnit(() => cleaner.Value = CreateCleaner()));
            Receive<StartCleanUp>(obs => 
                                      (
                                          from evt in obs
                                          from actor in cleaner 
                                          select (actor, evt.Event)
                                      )
                                     .ToUnit(p => p.actor.Forward(p.Event)));

            Receive<IRepositoryAction>(obs => obs.Select(d => new
                                                              {
                                                                  Actor = Context.ActorOf(() => new OperatorActor(d.State.RepositoryData, d.State.GridFsBucket,
                                                                                                                  d.State.TrashBin, d.State.DataTransferManager)),
                                                                  d.Event
                                                              })
                                                 .ToUnit(evt => evt.Actor.Forward(evt.Event)));

            Receive<IndexRequest>(obs => obs.ToUnit(p =>
                                                    {
                                                        try
                                                        {
                                                            p.State.RepositoryData.Indexes.CreateOne(new CreateIndexModel<RepositoryEntry>(Builders<RepositoryEntry>.IndexKeys.Ascending(r => r.RepoName)));
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            Log.Error(e, "Error on Building Repository index");
                                                        }
                                                    }));

            SupervisorStrategy = SupervisorStrategy.StoppingStrategy;
            Self.Tell(new IndexRequest());
        }

        private sealed record IndexRequest;
    }
}
