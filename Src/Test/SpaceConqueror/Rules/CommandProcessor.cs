using NRules.Fluent.Dsl;
using NRules.RuleModel;
using SpaceConqueror.Rules.Manager;
using SpaceConqueror.States;

namespace SpaceConqueror.Rules;

public sealed class CommandProcessor : Rule, IManager<CommandProcessor, CommandProcessorState>
{
    public override void Define()
    {
        CommandProcessorState state = null!;

        When().Match(() => state, s => !s.Commands.IsEmpty && s.Run);

        Then().Yield(ctx => SupplyCommands(ctx, state));
    }

    private CommandProcessorState SupplyCommands(IContext ctx, CommandProcessorState state)
    {
        ctx.InsertAll(state.Commands);
        return state with{ Commands = state.Commands.Clear(), Run = false };
    }
}