using Akkatecture.Aggregates;
using Akkatecture.Core;

namespace Akkatecture.Sagas;

public interface
    ISagaIsStartedByAsync<TAggregate, in TIdentity, in TAggregateEvent> : ISagaHandlesAsync<TAggregate, TIdentity,
        TAggregateEvent>
    where TAggregateEvent : class, IAggregateEvent<TAggregate, TIdentity>
    where TAggregate : IAggregateRoot<TIdentity>
    where TIdentity : IIdentity { }