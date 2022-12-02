using Microsoft.Extensions.Hosting;

namespace Tauron.TextAdventure.Engine.Core;

public sealed class GameHostingService<TGame> : BackgroundService
    where TGame : GameBase
{
    private readonly TGame _game;

    public GameHostingService(TGame game)
        => _game = game;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        => await _game.Run(stoppingToken).ConfigureAwait(false);
}