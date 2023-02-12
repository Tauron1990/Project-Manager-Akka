using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.GridFS.Core;

public sealed record GridFsContext(GridFSBucket Bucket, GridFSFileInfo? File, PathInfo OriginalPath, IDirectory? Parent)
{
    public GridFSFileInfo FindEntry(ObjectId id)
        => Bucket.Find(Builders<GridFSFileInfo>.Filter.Eq("_id", id)).Single();

    public GridFSFileInfo? FindEntry(string fileName)
        => Bucket.Find(Builders<GridFSFileInfo>.Filter.Eq(f => f.Filename, fileName)).SingleOrDefault();
}