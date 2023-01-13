using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace Tauron.TextAdventure.Engine.GamePackages.Fetchers;

public sealed class DirectoryLoader : PackageFetcherBase
{
    private readonly string _path;

    public DirectoryLoader(string basePath, string path)
        => _path = Path.IsPathRooted(path) ? path : Path.GetFullPath(path, basePath);

    public override async IAsyncEnumerable<GamePackage> Load(IHostEnvironment environment)
    {
        if(!System.IO.Directory.Exists(_path)) yield break;

        foreach (string directory in System.IO.Directory.EnumerateDirectories(_path))
        {
            foreach (string file in System.IO.Directory.EnumerateFiles(directory, "*Plugin.dll"))
            {
                IGamePackageFetcher loader = File(file);

                await foreach (GamePackage pack in loader
                                  .Load(
                                       new HostingEnvironment
                                       {
                                           ApplicationName = environment.ApplicationName,
                                           EnvironmentName = environment.EnvironmentName,
                                           ContentRootPath = directory,
                                           ContentRootFileProvider = new PhysicalFileProvider(directory),
                                       })
                                  .ConfigureAwait(false))
                    yield return pack;
            }
        }
    }
}