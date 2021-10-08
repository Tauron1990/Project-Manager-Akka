using System;
using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public static class GenericPathHelper
{
    public const char GenericSeperator = '/';

    public const string AbsolutPathMarker = "::";

    public static bool IsAbsolute(string path)
    {
        ReadOnlySpan<char> str = path;

        #pragma warning disable EPS06
        var index = str.IndexOf(AbsolutPathMarker, StringComparison.Ordinal);

        return index > 0 && !str[..index].Contains(GenericSeperator);
        #pragma warning restore EPS06
    }

    public static FilePath ToRelativePath(FilePath path)
    {
        if (path.Kind == PathType.Relative) return path;
        
        ReadOnlySpan<char> str = path.Path;

        #pragma warning disable EPS06
        var index = str.IndexOf(AbsolutPathMarker, StringComparison.Ordinal);
        #pragma warning restore EPS06

        return index == -1 ? path with { Kind = PathType.Relative } : new FilePath(str[..index].ToString(), PathType.Relative);
    }
    
    public static FilePath ChangeExtension(FilePath path, string newExtension)
        => Path.ChangeExtension(path, newExtension);

    public static FilePath Combine(FilePath first, FilePath secund)
    {
        var result = Path.Combine(first, secund);

        return Path.DirectorySeparatorChar != Path.AltDirectorySeparatorChar 
            ? result.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) 
            : result;
    }

    public static FilePath NormalizePath(FilePath path)
        => path with{ Path = path.Path.Replace('\\', GenericSeperator) };
}