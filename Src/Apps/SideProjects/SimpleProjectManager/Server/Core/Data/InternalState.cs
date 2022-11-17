using Akkatecture.Aggregates;
using Akkatecture.Aggregates.Snapshot;
using Akkatecture.Core;

namespace SimpleProjectManager.Server.Core.Data;

public abstract class InternalState<TAggregate, TIdentity, TSnapshot> : AggregateState<TAggregate, TIdentity>, ISnapshotAggregateState<TAggregate, TIdentity, TSnapshot>
    where TIdentity : IIdentity
    where TAggregate : IAggregateRoot<TIdentity>
    where TSnapshot : IAggregateSnapshot<TAggregate, TIdentity>
{
    public abstract void Hydrate(TSnapshot aggregateSnapshot);
    public abstract TSnapshot CreateSnapshot();
}