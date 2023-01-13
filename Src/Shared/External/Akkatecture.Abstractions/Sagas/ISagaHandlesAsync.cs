using Akkatecture.Aggregates;
using Akkatecture.Core;

namespace Akkatecture.Sagas;

public interface ISagaHandlesAsync<TAggregate, in TIdentity, in TAggregateEvent> : ISaga
    where TAggregateEvent : class, IAggregateEvent<TAggregate, TIdentity>
    where TAggregate : IAggregateRoot<TIdentity>
    where TIdentity : IIdentity
{
    Task HandleAsync(IDomainEvent<TAggregate, TIdentity, TAggregateEvent> domainEvent);
}