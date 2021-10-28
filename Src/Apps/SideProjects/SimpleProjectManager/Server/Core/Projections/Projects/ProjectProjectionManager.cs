using Akka.Actor;
using Akka.Persistence.MongoDb.Journal;
using Akka.Persistence.MongoDb.Query;
using LiquidProjections;
using SimpleProjectManager.Server.Core.Data;
using SimpleProjectManager.Server.Core.Data.Events;
using SimpleProjectManager.Server.Core.Projections.Core;
using SimpleProjectManager.Shared;
using Tauron.Application.MongoExtensions;

namespace SimpleProjectManager.Server.Core.Projections;

public sealed class ProjectProjectionManager : ProjectionManagerBase
{
    public void Initialize(ActorSystem system)
    {
        ImmutableListSerializer<ProjectFileId>.Register();

        InitializeDispatcher<ProjectProjection, Project, ProjectId>(
            system,
            map =>
            {
                map.Map<NewProjectCreatedEvent>(
                    b =>
                    {
                        b.AsCreateOf(e => e.AggregateIdentity)
                           .HandlingDuplicatesUsing((_,_,_) => true)
                           .Using(
                                (projection, evt, _) =>
                                {
                                    projection.Id = evt.AggregateIdentity;
                                    projection.JobName = evt.AggregateEvent.Name;

                                    return Task.CompletedTask;
                                });
                    });
                map.Map<ProjectDeadLineChangedEvent>(
                    b =>
                    {
                        b.AsUpdateOf(e => e.AggregateIdentity)
                           .CreatingIfMissing()
                           .Using(
                                (projection, evt, _) =>
                                {
                                    projection.Deadline = evt.AggregateEvent.Deadline;

                                    return Task.CompletedTask;
                                });
                    });
            });
    }
}