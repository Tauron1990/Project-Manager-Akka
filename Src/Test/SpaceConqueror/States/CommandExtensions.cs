using System.Linq.Expressions;
using NRules.Fluent.Dsl;

namespace SpaceConqueror.States;

public static class CommandExtensions
{
    public static ILeftHandSideExpression MatchCommand<TCommand>(this ILeftHandSideExpression expression, Expression<Func<TCommand>>? commands = null)
        where TCommand : IGameCommand
        => commands is null ? expression.Match<TCommand>() : expression.Match(commands);

    public static ILeftHandSideExpression MatchCommands(this ILeftHandSideExpression expression, Expression<Func<CommandProcessorState>> commands)
        => expression.Match(commands);

    public static void AddCommand(this CommandProcessorState state, IGameCommand command)
        => state.Commands = state.Commands.Add(command);
}