using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tauron.TextAdventure.Engine.Systems.Rooms.Builders;
using Tauron.TextAdventure.Engine.Systems.Rooms.Core;

namespace Tauron.TextAdventure.Engine.Core;

internal sealed class GameHostingService<TGame> : BackgroundService
    where TGame : GameBase
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly TGame _game;

    internal GameHostingService(IHostApplicationLifetime lifetime, IServiceProvider serviceProvider, TGame game)
    {
        _lifetime = lifetime;
        _serviceProvider = serviceProvider;
        _game = game;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ConstructRooms();

        await _game.Run(_serviceProvider, stoppingToken).ConfigureAwait(false);
        _lifetime.StopApplication();
    }

    private void ConstructRooms()
    {
        var assetManager = _serviceProvider.GetRequiredService<AssetManager>();
        
        if(GameHost.LoadContext is null)
            throw new InvalidOperationException("No Load Context Provided, This Should not be Possible");

        foreach (var action in GameHost.LoadContext.PostConfigServices)
            action(_serviceProvider);

        var map = _serviceProvider.GetRequiredService<RoomMap>();

        foreach (var builderPair in GameHost.LoadContext.Rooms)
        {
            try
            {
                RoomBuilderBase builder = builderPair.Value();
                if(GameHost.LoadContext.RoomModify.TryGetValue(builderPair.Key, out var modify))
                    modify(builder);

                map.Add(builderPair.Key, builder.CreateRoom(assetManager));
            }
            catch (Exception e)
            {
                throw new RoomCreationException(builderPair.Key, e);
            }
        }

        GameHost.LoadContext = null;
    }
}