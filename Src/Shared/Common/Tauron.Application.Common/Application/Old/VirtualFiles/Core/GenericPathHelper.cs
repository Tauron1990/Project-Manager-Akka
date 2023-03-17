using System;
using System.Diagnostics;
using System.IO;

namespace Tauron.Application.VirtualFiles.Core;

#pragma warning disable EPS06

[PublicAPI]
public static class GenericPathHelper
{
    public const char GenericSeperator = '/';

    public const string AbsolutPathMarker = "::";

    public static bool HasScheme(in PathInfo info, string scheme)
    {
        var span = info.Path.AsSpan();

        return span.StartsWith(scheme, StringComparison.Ordinal) && span[scheme.Length..].StartsWith(AbsolutPathMarker, StringComparison.Ordinal);
    }

    [DebuggerStepThrough]
    public static bool IsAbsolute(string path)
    {
        ReadOnlySpan<char> str = path;

        int index = str.IndexOf(AbsolutPathMarker, StringComparison.Ordinal);

        return index > 0 && !str[..index].Contains(GenericSeperator);
    }

    public static PathInfo ToRelativePath(in PathInfo path, out string scheme)
    {
        if(path.Kind == PathType.Relative)
        {
            scheme = string.Empty;

            return path;
        }

        ReadOnlySpan<char> str = path.Path;

        int index = str.IndexOf(AbsolutPathMarker, StringComparison.Ordinal);

        scheme = str[..index].ToString();

        return index == -1 ? path with { Kind = PathType.Relative } : new PathInfo(str[(index + 2)..].ToString(), PathType.Relative);
    }

    public static PathInfo ToRelativePath(in PathInfo info)
        => ToRelativePath(info, out _);

    public static PathInfo ChangeExtension(in PathInfo path, string newExtension)
        => Path.ChangeExtension(path, newExtension);

    public static PathInfo Combine(in PathInfo first, in PathInfo secund)
    {
        string result = Path.Combine(first, secund);

        return Path.DirectorySeparatorChar != Path.AltDirectorySeparatorChar
            ? result.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            : result;
    }

    public static PathInfo NormalizePath(in PathInfo path)
        => path with { Path = NormalizeString(path.Path) };

    public static string NormalizeString(string path)
        => path.Replace('\\', GenericSeperator);

    
    
    /*public static (PathInfo Path, PathInfo Head) SplitPathHead(PathInfo path)
    {
        var isAbsolut = path.Kind == PathType.Absolute;
        var scheme = string.Empty;
        var workingPath = (isAbsolut ? ToRelativePath(path, out scheme) : path).Path.AsSpan();

        if (workingPath.Contains(GenericSeperator)) { }

        var head = Path.GetDirectoryName(workingPath);
        var rest = workingPath[..^head.Length];

        PathInfo restInfo = isAbsolut ? $"{scheme}{AbsolutPathMarker}{rest.ToString()}" : rest.ToString();
        PathInfo headInfo = head.ToString();

        return (restInfo, headInfo);
    }*/
    public static string GetName(in PathInfo pathInfo)
    {
        PathInfo path = NormalizePath(pathInfo);

        int index = path.Path.LastIndexOf(GenericSeperator);

        if(index == -1)
            return path.Path;

        if(index + 1 >= 0 && index + 1 <= path.Path.Length)
            return path.Path[(index + 1)..];

        return path.Path;
    }
}