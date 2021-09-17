using System;
using System.IO;
using JetBrains.Annotations;
using MongoDB.Driver.GridFS;
using Tauron.Application.Files.VirtualFiles;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.GridFS
{
    [PublicAPI]
    public sealed class GridFSFile : GridFSSystemNode, IFile
    {
        public GridFSFile(GridFSBucket bucket, GridFSFileInfo? fileInfo, IDirectory? parentDirectory, string name, string path, Action? existsNow)
            : base(bucket, fileInfo, parentDirectory, name, path, existsNow)
        {
        }

        public override bool IsDirectory => false;

        public string Extension
        {
            get => Path.GetExtension(SafeFileInfo.Filename);
            set => Bucket.Rename(SafeFileInfo.Id, PathHelper.ChangeExtension(SafeFileInfo.Filename, value));
        }

        public long Size => SafeFileInfo.Length;

        public Stream Open(FileAccess access)
        {
            switch (access)
            {
                case FileAccess.Read:
                    return Bucket.OpenDownloadStream(SafeFileInfo.Id);
                case FileAccess.Write:
                    return Create();
                default:
                    throw new NotSupportedException($"{access} is Not Suspportet");
            }
        }

        public Stream Create()
        {
            if (FileInfo == null)
                return new UpdateStream(this, Bucket.OpenUploadStream(OriginalPath));

            Bucket.Delete(SafeFileInfo.Id);
            return new UpdateStream(this, Bucket.OpenUploadStream(SafeFileInfo.Filename));
        }

        public Stream CreateNew()
        {
            if (FileInfo != null)
                throw new IOException($"{OriginalPath} - File already Exist");
            return new UpdateStream(this, Bucket.OpenUploadStream(SafeFileInfo.Filename));
        }

        public IFile MoveTo(string location)
        {
            Bucket.Rename(SafeFileInfo.Id, location);
            FindEntry(SafeFileInfo.Id);
            return this;
        }

        private sealed class UpdateStream : Stream
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
                var id = _upload.Id;
                _upload.Close();
                _upload.Dispose();
                _file.FindEntry(id);
                base.Dispose(disposing);
            }
        }
    }
}