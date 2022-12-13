using System.Collections.Immutable;
using System.Reactive.Disposables;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.Data;
using Tauron.TextAdventure.Engine.GamePackages;
using Tauron.TextAdventure.Engine.Systems;
using Tauron.TextAdventure.Engine.Systems.Rooms;
using Tauron.TextAdventure.Engine.UI;
using Tauron.TextAdventure.Engine.UI.Internal;

namespace Tauron.TextAdventure.Engine;

[PublicAPI]
public sealed class GameHost
{
    internal static ImmutableList<PackageElement> PostConfig = ImmutableList<PackageElement>.Empty;

    internal static string RootDic { get; set; } = string.Empty;
    
    public static async ValueTask<IHost> Create<TGame>(string[] args)
        where TGame : GameBase, new()
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

        var game = new TGame();

        RootDic = game.ContentRoot;
        
        game.ConfigurateHost(hostBuilder);

        var elements = await RunLoader(game).ConfigureAwait(false);

        PostConfig = elements;
        
        hostBuilder
           .ConfigureHostOptions((c, _) => c.HostingEnvironment.ApplicationName = game.AppName)
           .UseEnvironment("Game")
           .UseContentRoot(game.ContentRoot)
           .ConfigureServices(
            c =>
            {
                c
                   .AddSingleton<RoomMap>()
                   .AddSingleton<EventManager>()
                   .AddSingleton(new AssetManager())
                   .AddTransient<MainMenu>()
                   .AddSingleton(game)
                   .AddSingleton<GameBase>(game)
                   .AddSingleton<IUILayer>(sp => sp.GetRequiredService<TGame>().CreateUILayer(sp))
                   .AddHostedService<GameHostingService<TGame>>();

                foreach (PackageElement element in elements)
                    element.Apply(c);
            });
        
        return hostBuilder.Build();
    }

    private static async ValueTask<ImmutableList<PackageElement>> RunLoader(GameBase game)
    {
        var enviroment = new HostingEnvironment
                         {
                             ApplicationName = game.AppName,
                             EnvironmentName = "Game",
                             ContentRootPath = game.ContentRoot,
                             ContentRootFileProvider = new PhysicalFileProvider(game.ContentRoot),
                         };
        
        var elements = ImmutableList<PackageElement>.Empty.AddRange(CorePack.LoadCore());
        var metas = new HashSet<Metadata>();

        await foreach (GamePackage element in game.CreateGamePackage().Load(enviroment).ConfigureAwait(false))
        {
            if(metas.Add(element.Metadata))
                elements = elements.AddRange(element.Content());
        }

        return elements;
    }

    public static async ValueTask RunGame(string toLoad, IServiceProvider serviceProvider)
    {
        AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            using var disposer = new CompositeDisposable();

            var eventManager = scope.ServiceProvider.GetRequiredService<EventManager>();
            var systems = scope.ServiceProvider.GetRequiredService<IEnumerable<ISystem>>();

            var store = new EventStore(toLoad);
            
            eventManager.Initialize(store);
            store.LoadGame();

            foreach (IDisposable system in systems.SelectMany(s => s.Initialize(eventManager)))
                disposer.Add(system);

            var menu = new RunningGame(scope.ServiceProvider);
            await menu.RunGame().ConfigureAwait(false);

            eventManager.Free();
        }
    }
}