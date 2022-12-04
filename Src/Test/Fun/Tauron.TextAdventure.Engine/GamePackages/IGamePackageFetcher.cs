namespace Tauron.TextAdventure.Engine.GamePackages;

public interface IGamePackageFetcher
{
    IAsyncEnumerable<GamePackage> Load();
}