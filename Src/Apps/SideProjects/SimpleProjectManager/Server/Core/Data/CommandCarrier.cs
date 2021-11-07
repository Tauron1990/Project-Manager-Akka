using Akkatecture.Aggregates;
using Akkatecture.Commands;
using Akkatecture.Core;

namespace SimpleProjectManager.Server.Core.Data;

public class CommandCarrier<TCommand, TAggregate, TId> : Command<TAggregate, TId>
    where TId : IIdentity where TAggregate : IAggregateRoot<TId>
{
    public TCommand Command { get; }

    public CommandCarrier(TCommand command, TId aggregateId) : base(aggregateId)
    {
        Command = command;
    }
}