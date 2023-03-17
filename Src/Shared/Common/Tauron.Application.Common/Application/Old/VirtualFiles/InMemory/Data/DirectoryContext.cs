using System.Reactive.PlatformServices;
using Tauron.Application.VirtualFiles.Core;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public sealed record DirectoryContext(InMemoryRoot Root, DirectoryContext? Parent, DirectoryEntry Data, PathInfo Path, ISystemClock Clock, InMemoryFileSystem RootSystem)
    : FileSystemContextBase<DirectoryEntry>(Root, Parent, Data, Path, Clock, RootSystem)
{
    public FileContext GetFileContext(DirectoryContext parent, FileEntry file, in PathInfo path)
        => new(Root, parent, file, path, Clock, RootSystem);

    public (FileContext FileContext, DirectoryContext NewContext) GetFileContext(DirectoryContext parent, in PathInfo file, in PathInfo path)
    {
        string fileName = file;
        
        (FileEntry Data, DirectoryEntry NewEntry) newFile = ActualData.GetOrAdd(file, () => Root.GetInitializedFile(fileName, Clock));
        var fileContext = new FileContext(
            Root,
            parent,
            newFile.Data,
            GenericPathHelper.Combine(path, file),
            Clock,
            RootSystem);

        return (fileContext, this with { Data = newFile.NewEntry });
    }
}