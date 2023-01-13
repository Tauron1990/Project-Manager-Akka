using System;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IVirtualFileSystem : IDirectory, IDisposable
{
    bool IsRealTime { get; }

    bool SaveAfterDispose { get; set; }

    PathInfo Source { get; }

    void Reload(PathInfo source);

    void Save();
}