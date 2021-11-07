using Akkatecture.Aggregates;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data.Events;

public sealed record ProjectNameChangedEvent(ProjectName NewName) : AggregateEvent<Project, ProjectId>;