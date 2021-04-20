using System;
using System.IO;
using JetBrains.Annotations;
using MongoDB.Driver.GridFS;
using Tauron.Application.Files.VirtualFiles;

namespace Tauron.Application.Files.GridFS
{
    public sealed class GridFSFile : GridFSSystemNode, IFile
    {
        public GridFSFile(GridFSBucket bucket, GridFSFileInfo fileInfo, IDirectory? parentDirectory, string name) 
            : base(bucket, fileInfo, parentDirectory, name)
        {
        }

        public override bool IsDirectory => false;

        public string Extension
        {
            get => Path.GetExtension(FileInfo.Filename);
            set => Bucket.Rename(FileInfo.Id, Path.ChangeExtension(FileInfo.Filename, value));
        }

        public long Size => FileInfo.Length;

        public Stream Open(FileAccess access)
        {
            switch (access)
            {
                case FileAccess.Read:
                    return Bucket.OpenDownloadStream(FileInfo.Id);
                case FileAccess.Write:
                    Bucket.Delete(FileInfo.Id);
                    var stream = Bucket.OpenUploadStream(FileInfo.Filename);
                    return stream;
                case FileAccess.ReadWrite:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(access), access, null);
            }
        }

        public Stream Create() => throw new NotImplementedException();

        public Stream CreateNew() => throw new NotImplementedException();

        public IFile MoveTo(string location) => throw new NotImplementedException();

        public sealed class UpdateStream : Stream
        {
            private readonly GridFSFile _file;
            private readonly GridFSUploadStream _upload;

            public UpdateStream(GridFSFile file, GridFSUploadStream upload)
            {
                _file = file;
                _upload = upload;
            }

            public override void Flush() => _upload.Flush();

            public override int Read(byte[] buffer, int offset, int count) => _upload.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin) => _upload.Seek(offset, origin);

            public override void SetLength(long value) => _upload.SetLength(value);

            public override void Write(byte[] buffer, int offset, int count) => _upload.Write(buffer, offset, count);

            public override bool CanRead => _upload.CanRead;

            public override bool CanSeek => _upload.CanSeek;

            public override bool CanWrite => _upload.CanWrite;

            public override long Length => _upload.Length;

            public override long Position
            {
                get => _upload.Position;
                set => _upload.Position = value;
            }

            protected override void Dispose(bool disposing)
            {
                _upload.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}