using System.Reflection;
using System.Runtime.Loader;

namespace Tauron.TextAdventure.Engine.GamePackages.Fetchers;

public sealed class FileFetcher : PackageFetcherBase
{
    private readonly string _file;

    public FileFetcher(string file)
        => _file = Path.GetFullPath(file);

    public override async IAsyncEnumerable<GamePackage> Load()
    {
        Assembly assembly = await Task.Run(() => AssemblyLoadContext.Default.LoadFromAssemblyPath(_file)).ConfigureAwait(false);

        await foreach (GamePackage package in Assembly(assembly).Load().ConfigureAwait(false))
            yield return package;
    }
}