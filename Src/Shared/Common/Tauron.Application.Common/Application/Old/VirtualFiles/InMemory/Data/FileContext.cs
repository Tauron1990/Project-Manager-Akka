using System.Reactive.PlatformServices;

namespace Tauron.Application.VirtualFiles.InMemory.Data;

public sealed record FileContext(InMemoryRoot Root,  FileEntry Data, PathInfo Path, ISystemClock Clock, InMemoryFileSystem RootSystem)
    : FileSystemContextBase<FileEntry>(Root, Data, Path, Clock, RootSystem);