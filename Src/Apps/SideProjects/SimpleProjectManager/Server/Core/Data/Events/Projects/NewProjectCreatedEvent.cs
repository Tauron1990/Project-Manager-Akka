
using Akkatecture.Aggregates;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Tags;

namespace SimpleProjectManager.Server.Core.Data.Events;

[NewElement]
public sealed record NewProjectCreatedEvent(ProjectName Name) : AggregateEvent<Project, ProjectId>;