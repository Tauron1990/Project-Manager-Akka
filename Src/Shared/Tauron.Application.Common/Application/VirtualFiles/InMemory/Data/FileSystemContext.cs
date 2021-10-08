using System.Reactive.PlatformServices;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public abstract record FileSystemContextBase<TData>(InMemoryRoot Root, IDirectory? Parent, TData Data, FilePath Path, ISystemClock Clock, IVirtualFileSystem RootSystem)
    where TData : IDataElement;

public sealed record FileContext(InMemoryRoot Root, IDirectory? Parent, FileEntry Data, FilePath Path, ISystemClock Clock, IVirtualFileSystem RootSystem) 
    : FileSystemContextBase<FileEntry>(Root, Parent, Data, Path, Clock, RootSystem);

public sealed record DirectoryContext(InMemoryRoot Root, IDirectory? Parent, DirectoryEntry Data, FilePath Path, ISystemClock Clock, IVirtualFileSystem RootSystem)
    : FileSystemContextBase<DirectoryEntry>(Root, Parent, Data, Path, Clock, RootSystem)
{
    public FileContext GetFileContext(IDirectory parent, FileEntry file, FilePath path) 
        => new(Root, parent, file, path, Clock, RootSystem);
    
    public FileContext GetFileContext(IDirectory parent, FilePath file, FilePath path) 
        => new(Root, parent, Root.GetInitializedFile(file, Clock), path, Clock, RootSystem);
}