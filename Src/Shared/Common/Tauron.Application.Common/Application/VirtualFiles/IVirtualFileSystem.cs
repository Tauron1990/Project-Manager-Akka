using System;
using JetBrains.Annotations;
using Tauron.Operations;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IVirtualFileSystem : IDirectory, IDisposable
{
    bool IsRealTime { get; }

    bool SaveAfterDispose { get; set; }

    PathInfo Source { get; }

    SimpleResult Reload(in PathInfo source);

    SimpleResult Save();
}