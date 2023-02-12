using JetBrains.Annotations;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Application.Files.GridFS.Core;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.GridFS;

[PublicAPI]
public static class GridFsExtensions
{
    public static IVirtualFileSystem GridFs(this VirtualFileFactory fac, in PathInfo path, GridFSBucket bucked)
        => new GirdFsSystem(new GridFsDirectory(new GridFsContext(bucked, File: null, path, Parent: null), NodeType.Root));

    public static IVirtualFileSystem GridFs(this VirtualFileFactory fac, MongoUrl url)
    {
        var mongoClient = new MongoClient(url);
        var bucket = new GridFSBucket(mongoClient.GetDatabase("VirtualFileSystem"));

        return GridFs(fac, new PathInfo("Root", PathType.Relative), bucket);
    }
}