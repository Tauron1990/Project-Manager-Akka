using MongoDB.Driver.GridFS;
using Tauron.Application.Files.VirtualFiles;

namespace Tauron.Application.Files.GridFS
{
    public static class FileSystemFactoryExtensions
    {
        public static IVirtualFileSystem CreateMongoDb(this VirtualFileFactory factory, GridFSBucket bucket)
            => new GridFSVistualFileSystem(bucket);
    }
}