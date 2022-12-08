using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using SpaceConqueror.Data.Assets;
using Tauron.TextAdventure.Engine.GamePackages;

namespace SpaceConqueror.Data;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)]
[UsedImplicitly]
public sealed class GameLoader : SinglePackage
{
    protected override string Name => "Basis";
    protected override Version Version => new(0, 1);

    protected override IEnumerable<PackageElement> LoadPack()
    {
        yield return PackageElement.Asset(AssetLoader.LoadAssets);
        yield return PackageElement.Translate("GameData");
    }
}