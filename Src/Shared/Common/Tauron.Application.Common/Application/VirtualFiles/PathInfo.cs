using System.Diagnostics;
using Tauron.Application.VirtualFiles.Core;

namespace Tauron.Application.VirtualFiles;

[DebuggerNonUserCode]
public sealed record PathInfo(string Path, PathType Kind)
{
    public static implicit operator string(PathInfo path)
        => GenericPathHelper.ToRelativePath(path).Path;

    public static implicit operator PathInfo(string path)
        => new(GenericPathHelper.NormalizeString(path), GenericPathHelper.IsAbsolute(path) ? PathType.Absolute : PathType.Relative);
}