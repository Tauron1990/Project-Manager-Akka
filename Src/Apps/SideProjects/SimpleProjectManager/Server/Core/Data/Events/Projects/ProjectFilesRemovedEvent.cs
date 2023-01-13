using System.Collections.Immutable;
using Akkatecture.Aggregates;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data.Events;

public sealed record ProjectFilesRemovedEvent(ImmutableList<ProjectFileId> Files) : AggregateEvent<Project, ProjectId>;