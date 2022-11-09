using NRules.Fluent.Dsl;
using SpaceConqueror.Rules.Manager;
using SpaceConqueror.States;
using SpaceConqueror.States.GameTime;

namespace SpaceConqueror.Rules;

public sealed class GameTimeProcessor : Rule, IManager<GameTimeProcessor, GameTime>
{
    public override void Define()
    {
        GameTime gameTime = null!;

        When()
           .MatchCommand<GameTimeTickCommand>()
           .MatchGameTime(() => gameTime);

        Then()
           .Do(ctx => UpdateGameTime(gameTime));
    }

    private void UpdateGameTime(GameTime gameTime)
    {
        TimeSpan lastUpdate = gameTime.Stopwatch.Elapsed;
        gameTime.Stopwatch.Restart();

        gameTime.Current += lastUpdate;
        gameTime.LastUpdate = lastUpdate;
    }
}