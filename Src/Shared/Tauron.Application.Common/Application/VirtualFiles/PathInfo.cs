using System.Diagnostics;
using Tauron.Application.VirtualFiles.Core;

namespace Tauron.Application.VirtualFiles;

public enum PathType
{
    Relative,
    Absolute
}

[DebuggerNonUserCode]
public sealed record PathInfo(string Path, PathType Kind)
{

    public static implicit operator string(PathInfo path)
        => path.Path;

    public static implicit operator PathInfo(string path) 
        => new(path, GenericPathHelper.IsAbsolute(path) ? PathType.Absolute : PathType.Relative);
}