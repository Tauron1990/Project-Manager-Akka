using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tauron.TextAdventure.Engine.UI;

namespace Tauron.TextAdventure.Engine;

[PublicAPI]
public sealed class GameHost
{
    public static IHost Create<TGame>(string[] args)
        where TGame : GameBase, new()
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

        var game = new TGame();

        hostBuilder.ConfigureServices(
            c =>
            {
                c.AddSingleton(game)
                   .AddSingleton<IUILayer>(sp => sp.GetRequiredService<TGame>().CreateUILayer(sp));
            });
        
        return hostBuilder.Build();
    }
}