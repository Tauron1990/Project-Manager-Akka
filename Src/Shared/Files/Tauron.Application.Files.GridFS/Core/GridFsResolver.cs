using System;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver.GridFS;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.GridFS.Core;

public class GridFsResolver : VirtualFiles.Resolvers.IFileSystemResolver
{
    public string Scheme => GirdFsSystem.Scheme;

    public IVirtualFileSystem? TryResolve(in PathInfo path, IServiceProvider services)
    {
        var bucked = services.GetService<GridFSBucket>();
        return bucked is null 
            ? null 
            : new GirdFsSystem(new GridFsDirectory(new GridFsContext(bucked, File: null, path, Parent: null), NodeType.Root));

    }
}