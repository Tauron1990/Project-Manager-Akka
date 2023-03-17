using System;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IVirtualFileSystem : IDirectory, IDisposable
{
    bool IsRealTime { get; }

    bool SaveAfterDispose { get; }

    [Pure]
    IVirtualFileSystem SetSaveAfterDispose(bool value);

    PathInfo Source { get; }

    [Pure]
    Result<IVirtualFileSystem> Reload(in PathInfo source);

    [Pure]
    Result<IVirtualFileSystem> Save();
}