using JetBrains.Annotations;
using Tauron.Application.Files.VirtualFiles.InMemory;
using Tauron.Application.Files.VirtualFiles.InMemory.Data;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.VirtualFiles
{
    [PublicAPI]
    public static class VirtualFileFactoryExtensions
    {
        public static IVirtualFileSystem CreateInMemory(this VirtualFileFactory unused, string name, DataDirectory? directory = null)
            => new InMemoryFileSystem("memory::", name, directory ?? new DataDirectory(name));

        public static IVirtualFileSystem CrerateLocal(this VirtualFileFactory unused, string path)
            => new LocalFileSystem.LocalFileSystem(Argument.NotNull(path, nameof(path)));
    }
}