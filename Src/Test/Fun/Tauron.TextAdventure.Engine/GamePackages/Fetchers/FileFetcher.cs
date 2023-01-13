using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace Tauron.TextAdventure.Engine.GamePackages.Fetchers;

public sealed class FileFetcher : PackageFetcherBase
{
    private readonly string _file;

    public FileFetcher(string file)
        => _file = Path.GetFullPath(file);

    public override async IAsyncEnumerable<GamePackage> Load(IHostEnvironment environment)
    {
        Assembly assembly = await Task.Run(() => AssemblyLoadContext.Default.LoadFromAssemblyPath(_file)).ConfigureAwait(false);

        if(!string.Equals(environment.ContentRootPath, Path.GetDirectoryName(_file), StringComparison.Ordinal))
        {
            string? dic = Path.GetDirectoryName(_file);
            if(!string.IsNullOrWhiteSpace(dic))
                environment = new HostingEnvironment
                              {
                                  ApplicationName = environment.ApplicationName,
                                  EnvironmentName = environment.EnvironmentName,
                                  ContentRootPath = dic,
                                  ContentRootFileProvider = new PhysicalFileProvider(dic),
                              };
        }

        await foreach (GamePackage package in Assembly(assembly).Load(environment).ConfigureAwait(false))
            yield return package;
    }
}