using System.Reactive.PlatformServices;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public abstract record FileSystemContextBase<TData>(InMemoryRoot Root, IDirectory? Parent, TData Data, string Path, ISystemClock Clock)
    where TData : IDataElement;

public sealed record FileContext(InMemoryRoot Root, IDirectory? Parent, FileEntry Data, string Path, ISystemClock Clock) 
    : FileSystemContextBase<FileEntry>(Root, Parent, Data, Path, Clock);

public sealed record DirectoryContext(InMemoryRoot Root, IDirectory? Parent, DirectoryEntry Data, string Path, ISystemClock Clock) 
    : FileSystemContextBase<DirectoryEntry>(Root, Parent, Data, Path, Clock);