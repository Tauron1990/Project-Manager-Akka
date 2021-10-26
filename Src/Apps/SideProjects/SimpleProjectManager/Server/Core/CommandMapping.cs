using Akkatecture.Aggregates;
using Akkatecture.Commands;
using Akkatecture.Core;

namespace SimpleProjectManager.Server.Core;

public sealed record CommandMapping(Type CommandType, Type AggregateManager)
{
    public static CommandMapping For<TAggregate, TId, TState, TManager, TCommand>()
        where TAggregate : AggregateRoot<TAggregate, TId, TState>
        where TId : IIdentity
        where TState : AggregateState<TAggregate, TId, IMessageApplier<TAggregate, TId>>
        where TCommand : Command<TAggregate, TId>
        where TManager : AggregateManager<TAggregate, TId, TCommand>
        => new(typeof(TCommand), typeof(TManager));
}