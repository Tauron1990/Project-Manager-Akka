namespace Tauron.Application.VirtualFiles.LocalVirtualFileSystem;

public abstract record ContextBase<TData>(string Root, bool NoParent, TData Data);