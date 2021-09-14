using System;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Application.Files.VirtualFiles;

namespace Tauron.Application.Files.GridFS
{
    public abstract class GridFSSystemNode : IFileSystemNode
    {
        private readonly Action? _existsNow;
        private GridFSFileInfo? _fileInfo;
        private readonly object _setLock = new();

        protected GridFSBucket Bucket { get; }

        protected GridFSFileInfo? FileInfo
        {
            get => _fileInfo;
            set
            {
                lock (_setLock)
                {
                    var existent = _fileInfo == null;
                    _fileInfo = value;

                    if (existent)
                        _existsNow?.Invoke();
                }
            }
        }

        protected GridFSFileInfo SafeFileInfo
        {
            get
            {
                lock (_setLock)
                {
                    if (FileInfo == null)
                        throw new FileNotFoundException(OriginalPath + " Not Found");

                    return FileInfo;
                }
            }
        }

        public string OriginalPath { get; }

        public DateTime LastModified => FileInfo?.UploadDateTime ?? DateTime.MinValue;

        public IDirectory? ParentDirectory { get; }

        public abstract bool IsDirectory { get; }

        public bool Exist => FileInfo != null;

        public string Name { get; }

        protected GridFSSystemNode(GridFSBucket bucket, GridFSFileInfo? fileInfo, IDirectory? parentDirectory, string name, string path, Action? existsNow)
        {
            _existsNow = existsNow;
            Bucket = bucket;
            _fileInfo = fileInfo;
            ParentDirectory = parentDirectory;
            Name = name;
            OriginalPath = path;
        }

        public virtual void Delete() => Bucket.Delete(SafeFileInfo.Id);

        public override string ToString() => $"MongoDB --- {OriginalPath}";

        protected void FindEntry(ObjectId id)
            => FileInfo = Bucket.Find(Builders<GridFSFileInfo>.Filter.Eq("_id", id)).Single();

        protected GridFSFileInfo FindEntry(string id)
            => Bucket.Find(Builders<GridFSFileInfo>.Filter.Eq(f => f.Filename, id)).SingleOrDefault();
    }
}