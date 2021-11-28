using Akkatecture.Aggregates;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data.Events;

public sealed record ProjectDeletedEvent : AggregateEvent<Project, ProjectId>;