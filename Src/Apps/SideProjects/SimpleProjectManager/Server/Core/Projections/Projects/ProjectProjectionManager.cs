﻿using Akka.Actor;
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
                            (projection, evt) =>
                            {
                                projection.Id = evt.AggregateIdentity;
                                projection.JobName = evt.AggregateEvent.Name;
                                projection.Ordering = new SortOrder(evt.AggregateIdentity, 0, false);
                            }));
                map.Map<ProjectDeadLineChangedEvent>(
                    b =>
                    {
                        b.AsUpdateOf(e => e.AggregateIdentity)
                           .Using((projection, evt) => projection.Deadline = evt.AggregateEvent.Deadline);
                    });

                map.Map<ProjectFilesAttachedEvent>(
                    b => b.AsUpdateOf(e => e.AggregateIdentity)
                       .Using((projection, evt) => projection.ProjectFiles = projection.ProjectFiles.AddRange(evt.AggregateEvent.Files)));

                map.Map<ProjectFilesRemovedEvent>(
                    b => b.AsUpdateOf(e => e.AggregateIdentity)
                       .Using((projection, evt) => projection.ProjectFiles = projection.ProjectFiles.RemoveRange(evt.AggregateEvent.Files)));
                
                map.Map<ProjectStatusChangedEvent>(
                    b => b.AsUpdateOf(e => e.AggregateIdentity)
                       .Using((projection, evt) => projection.Status = evt.AggregateEvent.NewStatus));

                map.Map<ProjectNameChangedEvent>(
                    b => b.AsUpdateOf(e => e.AggregateIdentity)
                       .Using((projection, evt) => projection.JobName = evt.AggregateEvent.NewName));

                map.Map<ProjectDeletedEvent>(
                    b => b.AsDeleteOf(de => de.AggregateIdentity).IgnoringMisses());
            });
    }

    public ProjectProjectionManager(ILoggerFactory loggerFactory) : base(loggerFactory) { }
}