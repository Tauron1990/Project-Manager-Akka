using System.IO;

namespace Tauron.Application.VirtualFiles.LocalVirtualFileSystem;

public sealed record DirectoryContext(string Root, bool NoParent, DirectoryInfo Data) : ContextBase<DirectoryInfo>(Root, NoParent, Data);