using System.Collections.Immutable;
using Microsoft.Extensions.Hosting;
using Tauron.TextAdventure.Engine.GamePackages;

namespace Tauron.TextAdventure.Engine.Core;

public sealed class GameHostingService<TGame> : BackgroundService
    where TGame : GameBase
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly TGame _game;

    public GameHostingService(IHostApplicationLifetime lifetime, IServiceProvider serviceProvider, TGame game)
    {
        _lifetime = lifetime;
        _serviceProvider = serviceProvider;
        _game = game;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach(PackageElement element in GameHost.PostConfig)
            element.PostConfig(_serviceProvider);
        
        GameHost.PostConfig = ImmutableList<PackageElement>.Empty;
        
        await _game.Run(_serviceProvider, stoppingToken).ConfigureAwait(false);
        _lifetime.StopApplication();
    }
}