using System;
using System.Reactive.PlatformServices;
using Tauron.Application.VirtualFiles.Core;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public abstract record FileSystemContextBase<TData>(InMemoryRoot Root, DirectoryContext? Parent, TData? Data, PathInfo Path, ISystemClock Clock, InMemoryFileSystem RootSystem)
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

public sealed record FileContext(InMemoryRoot Root, DirectoryContext? Parent, FileEntry Data, PathInfo Path, ISystemClock Clock, InMemoryFileSystem RootSystem) 
    : FileSystemContextBase<FileEntry>(Root, Parent, Data, Path, Clock, RootSystem);

public sealed record DirectoryContext(InMemoryRoot Root, DirectoryContext? Parent, DirectoryEntry Data, PathInfo Path, ISystemClock Clock, InMemoryFileSystem RootSystem)
    : FileSystemContextBase<DirectoryEntry>(Root, Parent, Data, Path, Clock, RootSystem)
{
    public FileContext GetFileContext(DirectoryContext parent, FileEntry file, PathInfo path) 
        => new(Root, parent, file, path, Clock, RootSystem);
    
    public FileContext GetFileContext(DirectoryContext parent, PathInfo file, PathInfo path)
        => new(Root, parent, ActualData.GetOrAdd(file, () => Root.GetInitializedFile(file, Clock)), GenericPathHelper.Combine(path, file), Clock, RootSystem);
}