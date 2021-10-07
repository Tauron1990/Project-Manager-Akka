using System;
using MHLab.Pooling;
using Microsoft.IO;

namespace Tauron.Application.VirtualFiles.InMemory.Data
{
    public sealed class InMemoryRoot : DirectoryEntry
    {
        private readonly RecyclableMemoryStreamManager _manager = new();
        private readonly Pool<FileEntry> _filePool = new(0, () => new FileEntry());
        private readonly Pool<DirectoryEntry> _directoryPool = new(0, () => new DirectoryEntry());

        public FileEntry GetInitializedFile(string name)
            => _filePool.Rent().Init(name, _manager.GetStream());

        public DirectoryEntry GetDirectoryEntry(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Name Should not be null or Empty");
            var dic = _directoryPool.Rent();
            dic.Name = name;

            return dic;
        }

        public void ReturnFile(FileEntry entry)
            => _filePool.Recycle(entry);

        public void ReturnDirectory(DirectoryEntry entry)
        {
            if (entry.GetType() != typeof(DirectoryEntry))
                throw new InvalidOperationException("Invalid Directory Returned");
            _directoryPool.Recycle(entry);
        }

        public override void Dispose()
        {
            base.Dispose();
            _filePool.Clear();
            _directoryPool.Clear();
        }
    }
}