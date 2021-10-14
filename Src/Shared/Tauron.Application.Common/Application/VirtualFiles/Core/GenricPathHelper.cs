using System;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles.Core;

#pragma warning disable EPS06

[PublicAPI]
public static class GenericPathHelper
{
    public const char GenericSeperator = '/';

    public const string AbsolutPathMarker = "::";

    public static bool HasScheme(PathInfo info, string scheme)
    {
        var span = info.Path.AsSpan();
        
        return span.StartsWith(scheme) && span[scheme.Length..].StartsWith(AbsolutPathMarker);
    }
    
    [DebuggerStepThrough]
    public static bool IsAbsolute(string path)
    {
        ReadOnlySpan<char> str = path;

        var index = str.IndexOf(AbsolutPathMarker, StringComparison.Ordinal);

        return index > 0 && !str[..index].Contains(GenericSeperator);
    }

    public static PathInfo ToRelativePath(PathInfo path)
    {
        if (path.Kind == PathType.Relative) return path;
        
        ReadOnlySpan<char> str = path.Path;
        
        var index = str.IndexOf(AbsolutPathMarker, StringComparison.Ordinal);

        return index == -1 ? path with { Kind = PathType.Relative } : new PathInfo(str[..index].ToString(), PathType.Relative);
    }
    
    public static PathInfo ChangeExtension(PathInfo path, string newExtension)
        => Path.ChangeExtension(path, newExtension);

    public static PathInfo Combine(PathInfo first, PathInfo secund)
    {
        var result = Path.Combine(first, secund);

        return Path.DirectorySeparatorChar != Path.AltDirectorySeparatorChar 
            ? result.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) 
            : result;
    }

    public static PathInfo NormalizePath(PathInfo path)
        => path with{ Path = path.Path.Replace('\\', GenericSeperator) };
}