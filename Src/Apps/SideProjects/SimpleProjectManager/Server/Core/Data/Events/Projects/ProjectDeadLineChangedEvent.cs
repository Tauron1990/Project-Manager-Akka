using Akkatecture.Aggregates;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data.Events;

public record ProjectDeadLineChangedEvent(ProjectDeadline? Deadline) : AggregateEvent<Project, ProjectId>;