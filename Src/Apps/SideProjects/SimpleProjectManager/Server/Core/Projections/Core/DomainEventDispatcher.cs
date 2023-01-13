using Akkatecture.Aggregates;

namespace SimpleProjectManager.Server.Core.Projections.Core;

public sealed class DomainEventDispatcher : AggregateEvent<IDomainEvent> { }