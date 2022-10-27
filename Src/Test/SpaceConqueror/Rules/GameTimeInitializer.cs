using NRules.Fluent.Dsl;
using NRules.RuleModel;
using SpaceConqueror.Rules.Manager;
using SpaceConqueror.States;
using SpaceConqueror.States.GameTime;

namespace SpaceConqueror.Rules;

public sealed class GameTimeInitializer : Rule, IManager<GameTimeInitializer, GameTime>
{
    public override void Define()
    {
        GameTime gameTime = null!;
        CommandProcessorState processorState = null!;

        When()
           .Match<InitializeGameCommand>()
           .MatchGameTime(() => gameTime)
           .MatchCommands(() => processorState);
        
        Then()
           .Do(ctx => StartTimer(ctx, gameTime))
           .Do(ctx => AddTickCommand(ctx, processorState));
    }

    private void AddTickCommand(IContext context, CommandProcessorState state)
        => context.Update(state.AddCommand(new GameTimeTickCommand()));
    
    
    private void StartTimer(IContext context, GameTime gameTime)
    {
        gameTime.Stopwatch.Restart();
        context.Update(gameTime);
    }
}