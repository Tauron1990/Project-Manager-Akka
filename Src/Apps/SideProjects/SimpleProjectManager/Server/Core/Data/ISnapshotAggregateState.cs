using Akkatecture.Aggregates;
using Akkatecture.Aggregates.Snapshot;
using Akkatecture.Core;

namespace SimpleProjectManager.Server.Core.Data;

public interface ISnapshotAggregateState<TAggregate, TIdentity, TSnapshot> : IHydrate<TSnapshot>
    where TSnapshot : IAggregateSnapshot<TAggregate, TIdentity>
    where TIdentity : IIdentity
    where TAggregate : IAggregateRoot<TIdentity>
{
    TSnapshot CreateSnapshot();
}