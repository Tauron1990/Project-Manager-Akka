using System.Reactive.PlatformServices;
using Tauron.Application.VirtualFiles.Core;
using Tauron.ObservableExt;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public sealed record DirectoryContext(
        InMemoryRoot Root, DirectoryContext? Parent, DirectoryEntry Data, PathInfo Path, ISystemClock Clock, InMemoryFileSystem RootSystem)
    : FileSystemContextBase<DirectoryEntry>(Root, Parent, Data, Path, Clock, RootSystem)
{
    public FileContext GetFileContext(DirectoryContext parent, FileEntry file, in PathInfo path)
        => new(Root, parent, file, path, Clock, RootSystem);

#pragma warning disable EPS05
    public Result<FileContext> GetFileContext(DirectoryContext parent, PathInfo file, PathInfo path)
#pragma warning restore EPS05
    {
        return
            from actualData in ActualData
            from newFile in actualData.GetOrAdd(file, () => Root.GetInitializedFile(file, Clock))
            select Result.Ok(new FileContext(Root, parent, newFile, GenericPathHelper.Combine(path, file), Clock, RootSystem));
    }
}