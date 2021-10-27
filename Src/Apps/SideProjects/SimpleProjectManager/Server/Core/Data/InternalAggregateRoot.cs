using Akka.Actor;
using Akka.Event;
using Akkatecture.Aggregates;
using Akkatecture.Aggregates.Snapshot;
using Akkatecture.Aggregates.Snapshot.Strategies;
using Akkatecture.Core;
using Tauron.Operations;

namespace SimpleProjectManager.Server.Core.Data;

public interface ISnapshotAggregateState<TAggregate, TIdentity, TSnapshot> : IHydrate<TSnapshot> 
    where TSnapshot : IAggregateSnapshot<TAggregate, TIdentity> 
    where TIdentity : IIdentity 
    where TAggregate : IAggregateRoot<TIdentity>
{
    TSnapshot CreateSnapshot();
}

public abstract class InternalState<TAggregate, TIdentity, TSnapshot> : AggregateState<TAggregate, TIdentity>, ISnapshotAggregateState<TAggregate, TIdentity, TSnapshot> 
    where TIdentity : IIdentity
    where TAggregate : IAggregateRoot<TIdentity> 
    where TSnapshot : IAggregateSnapshot<TAggregate, TIdentity> 
{
    public abstract void Hydrate(TSnapshot aggregateSnapshot);
    public abstract TSnapshot CreateSnapshot();
}

public abstract class InternalAggregateRoot<TAggregate, TIdentity, TAggregateState, TSnapshot> : AggregateRoot<TAggregate, TIdentity, TAggregateState>
    where TAggregateState : AggregateState<TAggregate, TIdentity, IMessageApplier<TAggregate, TIdentity>>, ISnapshotAggregateState<TAggregate, TIdentity, TSnapshot>
    where TIdentity : IIdentity
    where TAggregate : AggregateRoot<TAggregate, TIdentity, TAggregateState>
    where TSnapshot : IAggregateSnapshot<TAggregate, TIdentity>
{

    protected InternalAggregateRoot(TIdentity id) : base(id)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        SetSnapshotStrategy(new SnapshotEveryFewVersionsStrategy(100));
    }

    protected bool Run<TCommand>(TCommand command, Func<TCommand, IOperationResult> runner)
    {
        try
        {
            var result = runner(command);
            if(!Sender.IsNobody())
                Sender.Tell(result);

            return true;
        }
        catch (Exception e)
        {
            if(!Sender.IsNobody())
                Sender.Tell(OperationResult.Failure(e));
            Context.GetLogger().Error(e, "Eoor on process Command {Command}", command);

            return false;
        }
    }

    protected override IAggregateSnapshot<TAggregate, TIdentity>? CreateSnapshot()
    {
        if (State == null) return null;
        return State.CreateSnapshot();
    }
}