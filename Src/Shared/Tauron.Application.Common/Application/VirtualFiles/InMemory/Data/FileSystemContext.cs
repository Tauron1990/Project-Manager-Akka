using System;
using System.Reactive.PlatformServices;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public abstract record FileSystemContextBase<TData>(InMemoryRoot Root, DirectoryContext? Parent, TData? Data, FilePath Path, ISystemClock Clock, InMemoryFileSystem RootSystem)
    where TData : class, IDataElement
{
    public TData ActualData
    {
        get
        {
            if (Data is null)
                throw new InvalidOperationException("Element does not Exist");

            return Data;
        }
    }
}

public sealed record FileContext(InMemoryRoot Root, DirectoryContext? Parent, FileEntry Data, FilePath Path, ISystemClock Clock, InMemoryFileSystem RootSystem) 
    : FileSystemContextBase<FileEntry>(Root, Parent, Data, Path, Clock, RootSystem);

public sealed record DirectoryContext(InMemoryRoot Root, DirectoryContext? Parent, DirectoryEntry Data, FilePath Path, ISystemClock Clock, InMemoryFileSystem RootSystem)
    : FileSystemContextBase<DirectoryEntry>(Root, Parent, Data, Path, Clock, RootSystem)
{
    public FileContext GetFileContext(DirectoryContext parent, FileEntry file, FilePath path) 
        => new(Root, parent, file, path, Clock, RootSystem);
    
    public FileContext GetFileContext(DirectoryContext parent, FilePath file, FilePath path) 
        => new(Root, parent, Root.GetInitializedFile(file, Clock), path, Clock, RootSystem);
}