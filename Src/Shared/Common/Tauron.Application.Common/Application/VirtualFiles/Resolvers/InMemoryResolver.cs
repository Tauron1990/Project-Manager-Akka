using System;
using System.Reactive.PlatformServices;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory;

namespace Tauron.Application.VirtualFiles.Resolvers;

public sealed class InMemoryResolver : IFileSystemResolver
{
    public string Scheme => "mem";

    public IVirtualFileSystem? TryResolve(in PathInfo path, IServiceProvider services)
        => !GenericPathHelper.HasScheme(path, Scheme) ? null : new InMemoryFileSystem(services.GetRequiredService<ISystemClock>(), path);
}