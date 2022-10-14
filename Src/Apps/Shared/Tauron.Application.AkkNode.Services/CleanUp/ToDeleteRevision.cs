using System;
using Stl.IO;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.AkkaNode.Services.CleanUp;

public sealed record ToDeleteRevision(CleanUpId Id, PathInfo FilePath)
{
    public ToDeleteRevision()
        : this(new PathInfo(string.Empty, PathType.Relative)) { }

    public ToDeleteRevision(PathInfo filePath)
        : this(CleanUpId.From(Guid.NewGuid().ToString("N")), filePath) { }
}