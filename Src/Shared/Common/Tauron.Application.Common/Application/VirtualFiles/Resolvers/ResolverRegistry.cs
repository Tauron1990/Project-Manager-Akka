using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Tauron.Application.VirtualFiles.Core;

namespace Tauron.Application.VirtualFiles.Resolvers;

public static class ResolverRegistry
{
    private static readonly ConcurrentBag<IFileSystemResolver> Resolvers = new(
        new IFileSystemResolver[]
        {
            new InMemoryResolver(),
            new LocalFileSystemResolver(),
        });

    [PublicAPI]
    public static void Register(IFileSystemResolver resolver)
        => Resolvers.Add(resolver);

    public static IVirtualFileSystem? TryResolve(in PathInfo path, IServiceProvider serviceProvider)
    {
        if(path.Kind != PathType.Absolute)
            return null;

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (IFileSystemResolver resolver in Resolvers)
        {
            if(GenericPathHelper.HasScheme(path, resolver.Scheme))
            {
                IVirtualFileSystem? system = resolver.TryResolve(path, serviceProvider);
                if(system is not null)
                    return system;
            }
        }
        
        return null;
    }
}