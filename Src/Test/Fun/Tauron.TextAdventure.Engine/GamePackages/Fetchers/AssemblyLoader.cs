using System.Reflection;

namespace Tauron.TextAdventure.Engine.GamePackages.Fetchers;

public class AssemblyLoader : PackageFetcherBase
{
    private readonly Assembly _assembly;

    public AssemblyLoader(Assembly assembly)
        => _assembly = assembly;

    public override async IAsyncEnumerable<GamePackage> Load()
    {

        foreach (IGamePackageFetcher? package in _assembly
                    .GetExportedTypes()
                    .Where(t => t.IsAssignableTo(typeof(IGamePackageFetcher)))
                    .Select(Activator.CreateInstance)
                    .Cast<IGamePackageFetcher>())

            await foreach (GamePackage pack in package.Load().ConfigureAwait(false))
                yield return pack;
    }
}