using System.Reactive.PlatformServices;
using Tauron.Application.VirtualFiles.Core;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public sealed record DirectoryContext(InMemoryRoot Root, DirectoryContext? Parent, DirectoryEntry Data, PathInfo Path, ISystemClock Clock, InMemoryFileSystem RootSystem)
    : FileSystemContextBase<DirectoryEntry>(Root, Parent, Data, Path, Clock, RootSystem)
{
    public FileContext GetFileContext(DirectoryContext parent, FileEntry file, in PathInfo path)
        => new(Root, parent, file, path, Clock, RootSystem);

#pragma warning disable EPS05
    public FileContext GetFileContext(DirectoryContext parent, PathInfo file, in PathInfo path)
#pragma warning restore EPS05
        => new(Root, parent, ActualData.GetOrAdd(file, () => Root.GetInitializedFile(file, Clock)), GenericPathHelper.Combine(path, file), Clock, RootSystem);
}