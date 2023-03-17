using System.IO;

namespace Tauron.Application.VirtualFiles.LocalVirtualFileSystem;

public sealed record FileContext(string Root, bool NoParent, FileInfo Data) : ContextBase<FileInfo>(Root, NoParent, Data);