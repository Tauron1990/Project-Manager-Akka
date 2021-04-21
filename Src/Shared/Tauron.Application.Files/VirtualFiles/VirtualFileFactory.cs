using JetBrains.Annotations;
using Tauron.Application.Files.VirtualFiles.InMemory;
using Tauron.Application.Files.VirtualFiles.InMemory.Data;

namespace Tauron.Application.Files.VirtualFiles
{
    [PublicAPI]
    public sealed class VirtualFileFactory
    {
        public IVirtualFileSystem CreateInMemory(string name, DataDirectory? directory = null)
            => new InMemoryFileSystem(string.Empty, name, directory ?? new DataDirectory(name));

        public IVirtualFileSystem CrerateLocal(string path)
            => new LocalFileSystem.LocalFileSystem(Argument.NotNull(path, nameof(path)));
    }
}