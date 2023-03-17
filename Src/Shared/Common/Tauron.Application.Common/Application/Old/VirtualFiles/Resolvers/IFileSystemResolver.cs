using System;

namespace Tauron.Application.VirtualFiles.Resolvers;

public interface IFileSystemResolver
{
    string Scheme { get; }

    IVirtualFileSystem? TryResolve(in PathInfo path, IServiceProvider services);
}