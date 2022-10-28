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

        Then().Do(ctx => SupplyCommands(ctx, state));
    }

    private void SupplyCommands(IContext ctx, CommandProcessorState state)
    {
        ctx.InsertAll(state.Commands);

        state.Run = false;
        state.Commands = state.Commands.Clear();
    }
}