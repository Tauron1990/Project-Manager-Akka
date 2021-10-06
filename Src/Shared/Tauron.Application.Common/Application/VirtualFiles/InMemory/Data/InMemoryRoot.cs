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

        public DirectoryEntry GetDirectoryEntry()
            => _directoryPool.Rent();

        public void ReturnFile(FileEntry entry)
            => _filePool.Recycle(entry);

        public void ReturnDirectory(DirectoryEntry entry)
            => _directoryPool.Recycle(entry);

        public override void Dispose()
        {
            base.Dispose();
            _filePool.Clear();
            _directoryPool.Clear();
        }
    }
}