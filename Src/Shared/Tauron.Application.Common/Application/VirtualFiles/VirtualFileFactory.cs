using System;
using System.Reactive.PlatformServices;
using JetBrains.Annotations;
using Tauron.Application.VirtualFiles.InMemory;
using Tauron.Application.VirtualFiles.LocalVirtualFileSystem;
using Tauron.Application.VirtualFiles.Resolvers;

namespace Tauron.Application.VirtualFiles;
#pragma warning disable CA1822

[PublicAPI]
public sealed class VirtualFileFactory
{
    public IVirtualFileSystem? TryResolve(PathInfo path, IServiceProvider serviceProvider)
        => ResolverRegistry.TryResolve(path, serviceProvider);
    
    public IVirtualFileSystem InMemory(ISystemClock clock)
        => new InMemoryFileSystem(clock, "mem");

    public IVirtualFileSystem Local(string rootPath)
        => new LocalSystem(rootPath);
}