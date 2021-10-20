using System.IO;

namespace Tauron.Application.VirtualFiles.LocalVirtualFileSystem;

public abstract record ContextBase<TData>(string Root, bool NoParent, TData Data);

public sealed record FileContext(string Root, bool NoParent, FileInfo Data) : ContextBase<FileInfo>(Root, NoParent, Data);

public sealed record DirectoryContext(string Root, bool NoParent, DirectoryInfo Data) : ContextBase<DirectoryInfo>(Root, NoParent, Data);