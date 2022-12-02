namespace Tauron.TextAdventure.Engine.GamePackages;

public interface IGamePackageFetcher
{
    IAsyncEnumerable<Gamepackage> Load();
}