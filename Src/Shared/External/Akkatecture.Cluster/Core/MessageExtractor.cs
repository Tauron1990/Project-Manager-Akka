using System;
using Akka.Cluster.Sharding;
using Akkatecture.Aggregates;
using Akkatecture.Commands;
using Akkatecture.Core;
using Akkatecture.Sagas;
using Akkatecture.Sagas.AggregateSaga;

namespace Akkatecture.Cluster.Core;

// ReSharper disable once UnusedTypeParameter
public class MessageExtractor<TAggregateSagaManager, TAggregateSaga, TIdentity, TSagaLocator> : HashCodeMessageExtractor
    where TAggregateSagaManager : IAggregateSagaManager<TAggregateSaga, TIdentity, TSagaLocator>
    where TAggregateSaga : IAggregateSaga<TIdentity>
    where TIdentity : SagaId<TIdentity>
    where TSagaLocator : class, ISagaLocator<TIdentity>, new()
{
    public MessageExtractor(int maxNumberOfShards)
        : base(maxNumberOfShards)
        => SagaLocator = new TSagaLocator();

    private TSagaLocator SagaLocator { get; }

    public override string EntityId(object message)
        => message switch
        {
            null => throw new ArgumentNullException(nameof(message)),
            IDomainEvent domainEvent => SagaLocator.LocateSaga(domainEvent).Value,
            _ => throw new ArgumentException("Message could cast to Domain Event", nameof(message)),
        };
}

public class MessageExtractor<TAggregate, TIdentity> : HashCodeMessageExtractor
    where TAggregate : IAggregateRoot<TIdentity>
    where TIdentity : IIdentity
{
    public MessageExtractor(int maxNumberOfShards)
        : base(maxNumberOfShards) { }

    public override string EntityId(object message)
        => message switch
        {
            null => throw new ArgumentNullException(nameof(message)),
            ICommand<TAggregate, TIdentity> command => command.AggregateId.Value,
            _ => throw new ArgumentException("Message could not cast to message", nameof(message)),
        };
}