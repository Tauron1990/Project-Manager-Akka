using System.Reactive.PlatformServices;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public sealed record FileContext(InMemoryRoot Root, DirectoryContext? Parent, FileEntry Data, PathInfo Path, ISystemClock Clock, InMemoryFileSystem RootSystem)
    : FileSystemContextBase<FileEntry>(Root, Parent, Data, Path, Clock, RootSystem);