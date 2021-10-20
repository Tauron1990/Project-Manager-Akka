using System.IO;

namespace Tauron.Application.VirtualFiles.LocalVirtualFileSystem;

public interface IFullFileStreamSupport
{
    Stream Open(FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options);
}