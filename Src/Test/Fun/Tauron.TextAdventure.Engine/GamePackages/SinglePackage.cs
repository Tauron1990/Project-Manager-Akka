using JetBrains.Annotations;

namespace Tauron.TextAdventure.Engine.GamePackages;

[PublicAPI]
public abstract class SinglePackage : IGamePackageFetcher
{
    protected abstract string Name { get; }
    
    protected abstract Version Version { get; }
    
    #pragma warning disable CS1998
    public async IAsyncEnumerable<GamePackage> Load()
        #pragma warning restore CS1998
    {
        yield return new GamePackage(new Metadata(Name, Version, Display: true), LoadPack);
    }

    protected abstract IEnumerable<PackageElement> LoadPack();
}