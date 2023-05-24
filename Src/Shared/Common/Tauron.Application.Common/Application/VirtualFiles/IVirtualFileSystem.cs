using System;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IVirtualFileSystem : IDirectory, IDisposable
{
    bool IsRealTime { get; }

    bool SaveWhenDispose { get; set; }

    PathInfo Source { get; }

    Result Reload(PathInfo source);

    Result Save();
}