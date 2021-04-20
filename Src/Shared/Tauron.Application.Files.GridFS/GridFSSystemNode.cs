using System;
using MongoDB.Driver.GridFS;
using Tauron.Application.Files.VirtualFiles;

namespace Tauron.Application.Files.GridFS
{
    public abstract class GridFSSystemNode : IFileSystemNode
    {
        protected GridFSBucket Bucket { get; }

        protected GridFSFileInfo FileInfo { get; set; }

        public string OriginalPath => FileInfo.Filename;

        public DateTime LastModified => FileInfo.UploadDateTime;

        public IDirectory? ParentDirectory { get; }

        public abstract bool IsDirectory { get; }

        public bool Exist => true;

        public string Name { get; }

        protected GridFSSystemNode(GridFSBucket bucket, GridFSFileInfo fileInfo, IDirectory? parentDirectory, string name)
        {
            Bucket = bucket;
            FileInfo = fileInfo;
            ParentDirectory = parentDirectory;
            Name = name;
        }

        public virtual void Delete() => Bucket.Delete(FileInfo.Id);

        public override string ToString() => FileInfo.Filename;
    }
}