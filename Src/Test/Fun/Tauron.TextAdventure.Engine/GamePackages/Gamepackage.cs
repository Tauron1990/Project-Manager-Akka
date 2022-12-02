namespace Tauron.TextAdventure.Engine.GamePackages;

public record Gamepackage(Metadata Metadata, Func<IEnumerable<PackageElement>> Content);