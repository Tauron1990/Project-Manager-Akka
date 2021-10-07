using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class VirtualFileSystemBase<TContext> : DirectoryBase<TContext>, IVirtualFileSystem
{
    protected VirtualFileSystemBase(TContext context, FileSystemFeature feature) : base(context, feature, NodeType.Root) { }

    public bool IsRealTime => Features.HasFlag(FileSystemFeature.RealTime);
        
    public bool SaveAfterDispose { get; set; }
        
    public abstract string Source { get; }
        
    public void Reload(string source)
    {
        ValidateFeature(FileSystemFeature.Reloading);
        ReloadImpl(Context);
    }

    public void Save()
    {
        ValidateFeature(FileSystemFeature.Save);
            
        SaveImpl(Context);
    }

    public void Dispose()
    {
        try
        {
            if (Features.HasFlag(FileSystemFeature.Save) && SaveAfterDispose)
                Save();
        }
        finally
        {
            DisposeImpl();
        }
    }

    protected abstract void SaveImpl(TContext context);
        
    protected abstract void DisposeImpl();

    protected abstract void ReloadImpl(TContext context);
}