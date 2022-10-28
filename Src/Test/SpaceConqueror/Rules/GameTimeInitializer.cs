using NRules.Fluent.Dsl;
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
           .MatchGameTime(() => gameTime, t => !t.Stopwatch.IsRunning)
           .MatchCommands(() => processorState);
        
        Then()
           .Do(ctx => StartTimer(gameTime))
           .Do(ctx => AddTickCommand(processorState));
    }

    private void AddTickCommand(CommandProcessorState state)
    {
        if(state.Commands.Contains(GameTimeTickCommand.Inst)) return;

        state.Commands = state.Commands.Add(GameTimeTickCommand.Inst);
    }


    private void StartTimer(GameTime gameTime)
        => gameTime.Stopwatch.Restart();
}