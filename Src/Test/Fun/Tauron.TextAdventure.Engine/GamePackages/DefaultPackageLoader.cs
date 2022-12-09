using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace Tauron.TextAdventure.Engine.GamePackages;

[PublicAPI]
public class DefaultPackageLoader : PackageFetcherBase
{
    public override IAsyncEnumerable<GamePackage> Load(IHostEnvironment environment)
    {
        IGamePackageFetcher self = Assembly(System.Reflection.Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("No Entry Assembly Found"));
        IGamePackageFetcher mods = Directory(environment.ContentRootPath, "Mods");
        IGamePackageFetcher? additional = AdditionalFetcher();

        return Group(environment, self, additional, mods);
    }

    protected virtual IGamePackageFetcher? AdditionalFetcher()
        => null;
}