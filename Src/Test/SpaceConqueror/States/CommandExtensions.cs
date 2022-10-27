using System.Linq.Expressions;
using NRules.Fluent.Dsl;

namespace SpaceConqueror.States;

public static class CommandExtensions
{
    public static ILeftHandSideExpression MatchCommands(this ILeftHandSideExpression expression, Expression<Func<CommandProcessorState>> commands)
        => expression.Match(commands);

    public static CommandProcessorState AddCommand(this CommandProcessorState state, IGameCommand command)
        => state with { Commands = state.Commands.Add(command) };

}