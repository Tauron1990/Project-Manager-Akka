using Tauron.Application.VirtualFiles.Core;

namespace Tauron.Application.VirtualFiles;

public enum PathType
{
    Relative,
    Absolute
}

public sealed record FilePath(string Path, PathType Kind)
{
    public static implicit operator string(FilePath path)
        => path.Path;

    public static implicit operator FilePath(string path) 
        => new(path, GenericPathHelper.IsAbsolute(path) ? PathType.Absolute : PathType.Relative);
}