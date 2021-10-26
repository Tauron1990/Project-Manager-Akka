using Akka.Actor;
using Akkatecture.Aggregates;
using Akkatecture.Core;
using Tauron.Operations;

namespace SimpleProjectManager.Server.Core.Data;

public class InternalAggregateRoot<TAggregate, TIdentity, TAggregateState> : AggregateRoot<TAggregate, TIdentity, TAggregateState>
    where TAggregateState : AggregateState<TAggregate, TIdentity, IMessageApplier<TAggregate, TIdentity>> 
    where TIdentity : IIdentity
    where TAggregate : AggregateRoot<TAggregate, TIdentity, TAggregateState> {
    
    public InternalAggregateRoot(TIdentity id) : base(id) { }

    protected void Run<TCommand>(TCommand command, Func<TCommand, IOperationResult> runner)
    {
        try
        {

        }
        catch (Exception e)
        {
            if(!Sender.IsNobody())
                Sender.Tell(OperationResult.Failure(e));
            throw;
        }
    }
}