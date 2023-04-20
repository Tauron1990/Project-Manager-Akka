using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using SpaceConqueror.Data.Assets;
using SpaceConqueror.Data.Rooms;
using Tauron.TextAdventure.Engine.GamePackages;

namespace SpaceConqueror.Data;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)]
[UsedImplicitly]
public sealed class GameLoader : SinglePackage
{
    protected override string Name => "Basis";
    protected override Version Version => new(0, 1);

    protected override IEnumerable<PackageElement> LoadPack(IHostEnvironment hostEnvironment)
    {
        yield return PackageElement.Asset(hostEnvironment, AssetLoader.LoadAssets);
        yield return PackageElement.Translate(hostEnvironment, "GameData");
        yield return RoomLoader.Load();
    }
}