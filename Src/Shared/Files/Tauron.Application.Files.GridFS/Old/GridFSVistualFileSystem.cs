using System;
using System.IO;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.GridFS.Old
{
    public sealed class GridFSVistualFileSystem : GridFsDic, IVirtualFileSystem
    {
        private const string RootName = "Mongo::";

        public GridFSVistualFileSystem(GridFSBucket bucket)
            : base(bucket, FindOrCreate(bucket), parentDirectory: null, RootName, RootName, existsNow: null) { }

        public void Dispose() { }

        public bool IsRealTime => false;
        public bool SaveAfterDispose { get; set; }
        PathInfo IVirtualFileSystem.Source => Source;
        public void Reload(PathInfo source) => throw new InvalidOperationException("Reloading Is not Supportet On MongoDb");

        public void Save() => throw new InvalidOperationException("Save Not Supported");

        public override NodeType Type => NodeType.Root;

        public string Source => Bucket.Database.Client.Settings.ToString();

        private static GridFSFileInfo FindOrCreate(GridFSBucket bucked)
        {
            while (true)
            {
                string fullName = Path.Combine(RootName, DicId);
                GridFSFileInfo? file = bucked.Find(Builders<GridFSFileInfo>.Filter.Eq(f => f.Filename, fullName)).FirstOrDefault();

                if (file != null) return file;

                bucked.UploadFromBytes(fullName, Array.Empty<byte>());
            }
        }
    }
}