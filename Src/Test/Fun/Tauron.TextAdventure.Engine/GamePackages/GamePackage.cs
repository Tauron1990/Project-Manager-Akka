namespace Tauron.TextAdventure.Engine.GamePackages;

public record GamePackage(Metadata Metadata, Func<IEnumerable<PackageElement>> Content);