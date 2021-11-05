using Akka.Actor;
using LiquidProjections;
using SimpleProjectManager.Server.Core.Data;
using SimpleProjectManager.Server.Core.Data.Events;
using SimpleProjectManager.Server.Core.Projections.Core;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron.Application.MongoExtensions;

namespace SimpleProjectManager.Server.Core.Projections;

public sealed class ProjectProjectionManager : ProjectionManagerBase, IInitializeProjection
{
    public void Initialize(ActorSystem system)
    {
        ImmutableListSerializer<ProjectFileId>.Register();

        InitializeDispatcher<ProjectProjection, Project, ProjectId>(
            system,
            map =>
            {
                map.Map<NewProjectCreatedEvent>(
                    b => b.AsCreateOf(e => e.AggregateIdentity)
                       .HandlingDuplicatesUsing((_, _, _) => true)
                       .Using(
                            (projection, evt, _) =>
                            {
                                projection.Id = evt.AggregateIdentity;
                                projection.JobName = evt.AggregateEvent.Name;
                                projection.Ordering = new SortOrder(evt.AggregateIdentity, 0, false);
                                
                                return Task.CompletedTask;
                            }));
                map.Map<ProjectDeadLineChangedEvent>(
                    b =>
                    {
                        b.AsUpdateOf(e => e.AggregateIdentity)
                           .Using(
                                (projection, evt, _) =>
                                {
                                    projection.Deadline = evt.AggregateEvent.Deadline;
                                    return Task.CompletedTask;
                                });
                    });

                map.Map<ProjectFilesAttachedEvent>(
                    b => b.AsUpdateOf(e => e.AggregateIdentity)
                       .Using(
                            (projection, evt, _) =>
                            {
                                projection.ProjectFiles = projection.ProjectFiles.AddRange(evt.AggregateEvent.Files);
                            }));

                map.Map<ProjectStatusChangedEvent>(
                    b => b.AsUpdateOf(e => e.AggregateIdentity)
                       .Using(
                            (projection, evt, _) =>
                            {
                                projection.Status = evt.AggregateEvent.NewStatus;
                            }));
            });
    }
}