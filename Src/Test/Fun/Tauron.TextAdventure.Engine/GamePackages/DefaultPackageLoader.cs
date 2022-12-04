using JetBrains.Annotations;

namespace Tauron.TextAdventure.Engine.GamePackages;

[PublicAPI]
public class DefaultPackageLoader : PackageFetcherBase
{
    public override IAsyncEnumerable<GamePackage> Load()
    {
        IGamePackageFetcher self = Assembly(System.Reflection.Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("No Entry Assembly Found"));
        IGamePackageFetcher mods = Directory("Mods");
        IGamePackageFetcher? additional = AdditionalFetcher();

        return Group(self, additional, mods);
    }

    protected virtual IGamePackageFetcher? AdditionalFetcher()
        => null;
}