using Microsoft.Extensions.Hosting;

namespace Tauron.TextAdventure.Engine.GamePackages;

public interface IGamePackageFetcher
{
    IAsyncEnumerable<GamePackage> Load(IHostEnvironment environment);
}