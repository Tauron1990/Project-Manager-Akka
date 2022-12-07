using Microsoft.Extensions.DependencyInjection;
using Tauron.TextAdventure.Engine.Core;

namespace Tauron.TextAdventure.Engine.GamePackages.Elements;

public sealed class AssetLoader : PackageElement
{
    private readonly Action<AssetManager> _configurator;

    public AssetLoader(Action<AssetManager> configurator)
        => _configurator = configurator;

    internal override void Apply(IServiceCollection serviceCollection)
    {
        
    }

    internal override void PostConfig(IServiceProvider serviceProvider)
        => _configurator(serviceProvider.GetRequiredService<AssetManager>());
}