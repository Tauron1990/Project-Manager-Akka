using System.Collections.Generic;
using Tauron.Application.VirtualFiles;
using Tauron.Application.VirtualFiles.Core;

namespace Tauron.Application.Files.Zip.Data;

public static class ZipExtensions
{
    public static IDirectory? CreateParent(this ZipContext context, IFileSystemNode from)
        => context.Parent is null ? null : new ZipDirectory(context.Parent, from.Features);

    public static PathInfo ConstructOriginalPath(this ZipContext context)
        => string.Join(GenericPathHelper.GenericSeperator, GetNameSegments(context));

    private static IEnumerable<string> GetNameSegments(ZipContext context)
    {
        if (context.Parent != null)
        {
            foreach (var nameSegment in GetNameSegments(context.Parent))
                yield return nameSegment;
        }

        yield return context.Name;
    }
}