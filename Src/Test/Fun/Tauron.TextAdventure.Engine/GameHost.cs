using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.GamePackages;
using Tauron.TextAdventure.Engine.UI;

namespace Tauron.TextAdventure.Engine;

[PublicAPI]
public sealed class GameHost
{
    public static async ValueTask<IHost> Create<TGame>(string[] args)
        where TGame : GameBase, new()
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

        var game = new TGame();

        var elements = ImmutableList<PackageElement>.Empty;
        var metas = ImmutableList<Metadata>.Empty;

        await foreach (Gamepackage element in game.CreateGamePackage().Load().ConfigureAwait(false))
        {
            elements = elements.AddRange(element.Content());
            metas = metas.Add(element.Metadata);
        }

        hostBuilder.ConfigureServices(
            c =>
            {
                c
                   .AddSingleton(game)
                   .AddSingleton<IUILayer>(sp => sp.GetRequiredService<TGame>().CreateUILayer(sp))
                   .AddHostedService<GameHostingService<TGame>>();

                foreach (PackageElement element in elements)
                {
                    element.Apply(c);
                }
            });
        
        return hostBuilder.Build();
    }
}