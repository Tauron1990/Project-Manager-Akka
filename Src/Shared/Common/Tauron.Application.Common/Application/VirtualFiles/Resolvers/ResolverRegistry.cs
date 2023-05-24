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

    public static Result<IVirtualFileSystem> TryResolve(in PathInfo path, IServiceProvider serviceProvider)
    {
        if(path.Kind != PathType.Absolute)
            return Result.Fail(new NotAbsoluteError(path));

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (IFileSystemResolver resolver in Resolvers)
            if(GenericPathHelper.HasScheme(path, resolver.Scheme))
                return resolver.TryResolve(path, serviceProvider);

        return Result.Fail(new RsolverNotMatch(path));
    }
}