using Microsoft.Extensions.DependencyInjection;
using Tauron.TextAdventure.Engine.Data;
using Tauron.TextAdventure.Engine.GamePackages.Core;
using Tauron.TextAdventure.Engine.Systems;

namespace Tauron.TextAdventure.Engine.GamePackages.Elements;

public sealed class InitGame : PackageElement
{
    private readonly Action<GameState> _init;

    public InitGame(Action<GameState> init)
        => _init = init;

    private  void PostConfig(IServiceProvider serviceProvider)
    {
        var man = serviceProvider.GetRequiredService<EventManager>();

        man.Init = man.Init.Add(_init);
    }

    internal override void Load(ElementLoadContext context)
        => context.PostConfigServices.Add(PostConfig);
}