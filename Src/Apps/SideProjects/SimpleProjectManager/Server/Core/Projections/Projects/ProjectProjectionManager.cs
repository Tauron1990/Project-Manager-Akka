using Akka.Actor;
using Akka.Persistence.MongoDb.Journal;
using Akka.Persistence.MongoDb.Query;
using LiquidProjections;
using SimpleProjectManager.Server.Core.Data;
using SimpleProjectManager.Server.Core.Data.Events;
using SimpleProjectManager.Shared;
using Tauron.Akkatecture.Projections;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.MongoExtensions;

namespace SimpleProjectManager.Server.Core.Projections;

public sealed class ProjectProjectionManager
{
    public void Initialize(ActorSystem system)
    {
        ImmutableListSerializer<ProjectFileId>.Register();

        var database = system.Settings.Config.GetString("akka.persistence.journal.mongodb.connection-string");

        var evtBuilder = new DomainEventMapBuilder<ProjectProjection, Project, ProjectId>();

        evtBuilder.Map<NewProjectCreatedEvent>(
            b =>
            {
                b.AsCreateOf(e => e.AggregateIdentity)
                   .OverwritingDuplicates().
                    Using((projection, evt, _) =>
                          {
                              projection.Id = evt.AggregateIdentity;
                              projection.JobName = evt.AggregateEvent.Name;
                              return Task.CompletedTask;
                          });
            });

        IProjectionRepository repository = null!;

        var projector = new DomainProjector(evtBuilder.Build(new RepositoryProjectorMap<ProjectProjection, ProjectId>(repository)));

        var dispatcher = new DomainDispatcher<ProjectProjection, ProjectId>(
            new AggregateEventReader<MongoDbReadJournal>(system, "akka.persistence.journal.mongodb"), projector, repository);

        dispatcher.Subscribe<Project>();
    }
}