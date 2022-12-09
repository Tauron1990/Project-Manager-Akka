using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace Tauron.TextAdventure.Engine.GamePackages;

public static class Extensions
{
    public static IHostEnvironment WithNewContentRoot(this IHostEnvironment environment, string path)
        => Path.IsPathRooted(path)
            ? new HostingEnvironment
              {
                  ApplicationName = environment.ApplicationName,
                  EnvironmentName = environment.EnvironmentName,
                  ContentRootPath = path,
                  ContentRootFileProvider = new PhysicalFileProvider(path),
              }
            : new HostingEnvironment
              {
                  ApplicationName = environment.ApplicationName,
                  EnvironmentName = environment.EnvironmentName,
                  ContentRootPath = Path.Combine(environment.ContentRootPath, path),
                  ContentRootFileProvider = new PhysicalFileProvider(Path.Combine(environment.ContentRootPath, path)),
              };

    public static IDirectoryContents ResolvePath(this IHostEnvironment enviroment, string fromPath)
        => Path.IsPathRooted(fromPath) ? new PhysicalDirectoryContents(fromPath) : enviroment.ContentRootFileProvider.GetDirectoryContents(fromPath);
}