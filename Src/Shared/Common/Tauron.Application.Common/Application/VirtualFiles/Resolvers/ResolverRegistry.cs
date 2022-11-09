using System;
using System.Collections.Concurrent;
using System.Linq;
using JetBrains.Annotations;
using Tauron.Application.VirtualFiles.Core;

namespace Tauron.Application.VirtualFiles.Resolvers;

public static class ResolverRegistry
{
    private static readonly ConcurrentBag<IFileSystemResolver> Resolvers = new(
        new IFileSystemResolver[]
        {
            new InMemoryResolver(),
            new LocalFileSystemResolver()
        });

    [PublicAPI]
    public static void Register(IFileSystemResolver resolver)
        => Resolvers.Add(resolver);

    public static IVirtualFileSystem? TryResolve(PathInfo path, IServiceProvider serviceProvider)
    {
        if(path.Kind != PathType.Absolute)
            return null;

        return Resolvers
           .Where(r => GenericPathHelper.HasScheme(path, r.Scheme))
           .Select(r => r.TryResolve(path, serviceProvider))
           .FirstOrDefault(f => f is not null);
    }
}