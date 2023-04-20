using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.GamePackages.Core;

namespace Tauron.TextAdventure.Engine.GamePackages.Elements;

public sealed class AssetLoader : PackageElement
{
    private readonly IHostEnvironment _environment;
    private readonly Action<IHostEnvironment, AssetManager> _configurator;

    public AssetLoader(IHostEnvironment environment, Action<IHostEnvironment, AssetManager> configurator)
    {
        _environment = environment;
        _configurator = configurator;
    }


    internal override void Load(ElementLoadContext context)
        => context.PostConfigServices.Add(s => _configurator(_environment, s.GetRequiredService<AssetManager>()));
}