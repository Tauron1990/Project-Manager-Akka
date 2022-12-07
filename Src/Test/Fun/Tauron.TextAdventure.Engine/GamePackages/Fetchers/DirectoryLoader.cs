namespace Tauron.TextAdventure.Engine.GamePackages.Fetchers;

public sealed class DirectoryLoader : PackageFetcherBase
{
    private readonly string _path;

    public DirectoryLoader(string path)
        => _path = Path.GetFullPath(path);

    public override async IAsyncEnumerable<GamePackage> Load()
    {
        if(!System.IO.Directory.Exists(_path)) yield break;
        
        foreach (string directory in System.IO.Directory.EnumerateDirectories(_path))
        {
            foreach (string file in System.IO.Directory.EnumerateFiles(directory, "*Plugin.dll"))
            {
                IGamePackageFetcher loader = File(file);

                await foreach (GamePackage pack in loader.Load().ConfigureAwait(false))
                    yield return pack;
            }
        }
    }
}