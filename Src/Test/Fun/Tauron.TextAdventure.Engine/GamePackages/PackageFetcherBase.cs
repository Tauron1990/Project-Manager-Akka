using System.Reflection;
using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.GamePackages.Fetchers;

namespace Tauron.TextAdventure.Engine.GamePackages;

[PublicAPI]
public abstract class PackageFetcherBase : IGamePackageFetcher
{
    public abstract IAsyncEnumerable<GamePackage> Load();

    public static IGamePackageFetcher File(string file)
        => new FileFetcher(file);

    public static IGamePackageFetcher Directory(string dic)
        => new DirectoryLoader(dic);

    public static IGamePackageFetcher Assembly(Assembly assembly)
        => new AssemblyLoader(assembly);

    public static async IAsyncEnumerable<GamePackage> Group(params IGamePackageFetcher?[] fetcher)
    {
        foreach (IGamePackageFetcher? packageFetcher in fetcher)
        {
            if(packageFetcher is null) continue;

            await foreach (GamePackage pack in packageFetcher.Load().ConfigureAwait(false))
                yield return pack;
        }
    }
}