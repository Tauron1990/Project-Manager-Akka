using Microsoft.Extensions.DependencyInjection;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.GamePackages.Core;

namespace Tauron.TextAdventure.Engine.GamePackages.Elements;

public sealed class AssetLoader : PackageElement
{
    private readonly Action<AssetManager> _configurator;

    public AssetLoader(Action<AssetManager> configurator)
        => _configurator = configurator;

    
    internal override void Load(ElementLoadContext context)
        => context.PostConfigServices.Add(s => _configurator(s.GetRequiredService<AssetManager>()));
}