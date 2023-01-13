using Akkatecture.Aggregates;
using Akkatecture.Core;

namespace Akkatecture.Subscribers;

public interface ISubscribeToAsync<TAggregate, in TIdentity, in TAggregateEvent>
    where TAggregate : IAggregateRoot<TIdentity>
    where TIdentity : IIdentity
    where TAggregateEvent : class, IAggregateEvent<TAggregate, TIdentity>
{
    Task HandleAsync(IDomainEvent<TAggregate, TIdentity, TAggregateEvent> domainEvent);
}