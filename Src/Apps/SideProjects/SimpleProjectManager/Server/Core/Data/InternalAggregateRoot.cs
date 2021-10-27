using Akka.Actor;
using Akka.Event;
using Akkatecture.Aggregates;
using Akkatecture.Core;
using Tauron.Operations;

namespace SimpleProjectManager.Server.Core.Data;

public class InternalAggregateRoot<TAggregate, TIdentity, TAggregateState> : AggregateRoot<TAggregate, TIdentity, TAggregateState>
    where TAggregateState : AggregateState<TAggregate, TIdentity, IMessageApplier<TAggregate, TIdentity>> 
    where TIdentity : IIdentity
    where TAggregate : AggregateRoot<TAggregate, TIdentity, TAggregateState> {
    
    public InternalAggregateRoot(TIdentity id) : base(id) { }

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
}