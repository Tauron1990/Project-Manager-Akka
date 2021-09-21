using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.GridFS
{
    public static class FileSystemFactoryExtensions
    {
        public static IVirtualFileSystem CreateMongoDb(this VirtualFileFactory factory, GridFSBucket bucket)
            => new GridFSVistualFileSystem(bucket);

        public static IVirtualFileSystem CreateMongoDb(this VirtualFileFactory factory, string bucket)
        {
            var mongoUrl = MongoUrl.Create(bucket);

            return new GridFSVistualFileSystem(new GridFSBucket(new MongoClient(bucket).GetDatabase(mongoUrl.DatabaseName)));
        }
    }
}