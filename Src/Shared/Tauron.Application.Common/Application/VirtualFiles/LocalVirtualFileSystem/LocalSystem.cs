using Tauron.Application.VirtualFiles.Core;

namespace Tauron.Application.VirtualFiles.LocalVirtualFileSystem;

public sealed class LocalSystem : DelegatingVirtualFileSystem<LocalSystem>
{
    public LocalSystem(string rootPath)
    {
        
    }
}