using System;
using System.IO;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.GridFS
{
    public sealed class GridFSVistualFileSystem : GridFsDic, IVirtualFileSystem
    {
        private const string RootName = "Mongo::";

        public GridFSVistualFileSystem(GridFSBucket bucket)
            : base(bucket, FindOrCreate(bucket), null, RootName, RootName, null) { }

        public void Dispose() { }

        public bool IsRealTime => false;
        public bool SaveAfterDispose { get; set; }
        public string Source => Bucket.Database.Client.Settings.ToString();
        public void Reload(string source) => throw new NotSupportedException("Reloading Is not Supportet On MongoDb");

        private static GridFSFileInfo FindOrCreate(GridFSBucket bucked)
        {
            while (true)
            {
                var fullName = Path.Combine(RootName, DicId);
                var file = bucked.Find(Builders<GridFSFileInfo>.Filter.Eq(f => f.Filename, fullName)).FirstOrDefault();

                if (file != null) return file;

                bucked.UploadFromBytes(fullName, Array.Empty<byte>());
            }
        }
    }
}