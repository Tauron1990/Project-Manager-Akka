using Akkatecture.Aggregates;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data.Events;

public sealed record ProjectStatusChangedEvent(ProjectStatus NewStatus) : AggregateEvent<Project, ProjectId>;