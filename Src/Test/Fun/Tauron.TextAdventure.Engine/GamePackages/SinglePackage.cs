using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace Tauron.TextAdventure.Engine.GamePackages;

[PublicAPI]
public abstract class SinglePackage : IGamePackageFetcher
{
    protected abstract string Name { get; }
    
    protected abstract Version Version { get; }
    
    private IHostEnvironment? HostEnvironment { get; set; }
    
    #pragma warning disable CS1998
    [MemberNotNull(nameof(HostEnvironment))]
    public async IAsyncEnumerable<GamePackage> Load(IHostEnvironment hostEnvironment)
        #pragma warning restore CS1998
    {
        HostEnvironment = hostEnvironment;
        yield return new GamePackage(new Metadata(Name, Version, Display: true), LoadPack);
    }
    
    private IEnumerable<PackageElement> LoadPack()
        => LoadPack(HostEnvironment!);

    protected abstract IEnumerable<PackageElement> LoadPack(IHostEnvironment hostEnvironment);
}