using System;
using System.Reactive.PlatformServices;
using Tauron.Application.VirtualFiles.InMemory;
using Tauron.Application.VirtualFiles.LocalVirtualFileSystem;
using Tauron.Application.VirtualFiles.Resolvers;

namespace Tauron.Application.VirtualFiles;
#pragma warning disable CA1822

[PublicAPI]
public sealed class VirtualFileFactory
{
    public static readonly VirtualFileFactory Shared = new();
    
    [Pure]
    public IVirtualFileSystem? TryResolve(in PathInfo path, IServiceProvider serviceProvider)
        => ResolverRegistry.TryResolve(path, serviceProvider);

    [Pure]
    public IVirtualFileSystem InMemory(ISystemClock clock)
        => new InMemoryFileSystem(clock, "mem");

    [Pure]
    public IVirtualFileSystem Local(string rootPath)
        => new LocalSystem(rootPath);
}