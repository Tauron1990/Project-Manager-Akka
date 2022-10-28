using NRules.Fluent.Dsl;
using NRules.RuleModel;
using SpaceConqueror.Rules.Manager;
using SpaceConqueror.States.GameTime;

namespace SpaceConqueror.Rules;

public sealed class GameTimeProcessor : Rule, IManager<GameTimeProcessor, GameTime>
{
    public override void Define()
    {
        GameTime gameTime = null!;
        
        When()
           .Match<GameTimeTickCommand>()
           .MatchGameTime(() => gameTime);

        Then()
           .Do(ctx => UpdateGameTime(ctx, gameTime));
    }

    private void UpdateGameTime(IContext ctx, GameTime gameTime)
    {
        TimeSpan lastUpdate = gameTime.Stopwatch.Elapsed;
        gameTime.Stopwatch.Restart();

        ctx.Update(gameTime with { Current = gameTime.Current + lastUpdate, LastUpdate = lastUpdate });
    }
}