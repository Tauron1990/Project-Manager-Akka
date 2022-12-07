using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.GamePackages;
using Tauron.TextAdventure.Engine.UI;
using Tauron.TextAdventure.Engine.UI.Internal;

namespace Tauron.TextAdventure.Engine;

[PublicAPI]
public sealed class GameHost
{
    internal static ImmutableList<PackageElement> PostConfig = ImmutableList<PackageElement>.Empty;
    
    public static async ValueTask<IHost> Create<TGame>(string[] args)
        where TGame : GameBase, new()
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args).UseConsoleLifetime();

        var game = new TGame();

        game.ConfigurateHost(hostBuilder);
        
        var elements = ImmutableList<PackageElement>.Empty;
        var metas = new HashSet<Metadata>();

        await foreach (GamePackage element in game.CreateGamePackage().Load().ConfigureAwait(false))
        {
            if(metas.Add(element.Metadata))
                elements = elements.AddRange(element.Content());
        }

        PostConfig = elements;
        
        hostBuilder.ConfigureServices(
            c =>
            {
                c
                   .AddSingleton<AssetManager>()
                   .AddTransient<MainMenu>()
                   .AddSingleton(game)
                   .AddSingleton<IUILayer>(sp => sp.GetRequiredService<TGame>().CreateUILayer(sp))
                   .AddHostedService<GameHostingService<TGame>>();

                foreach (PackageElement element in elements)
                    element.Apply(c);
            });
        
        return hostBuilder.Build();
    }
}