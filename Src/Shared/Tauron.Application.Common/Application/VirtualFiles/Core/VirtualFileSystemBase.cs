using System;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class VirtualFileSystemBase<TContext> : DirectoryBase<TContext>, IVirtualFileSystem
{
    protected VirtualFileSystemBase(TContext context, FileSystemFeature feature) 
        : base(context, feature, NodeType.Root) { }

    protected VirtualFileSystemBase(Func<IFileSystemNode, TContext> context, FileSystemFeature feature) 
        : base(context, feature, NodeType.Root) { }
    
    public bool IsRealTime => Features.HasFlag(FileSystemFeature.RealTime);
        
    public bool SaveAfterDispose { get; set; }
        
    public abstract FilePath Source { get; }
        
    public void Reload(FilePath source)
    {
        ValidateFeature(FileSystemFeature.Reloading);
        ReloadImpl(Context, source);
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

    protected virtual void SaveImpl(TContext context)
        => throw new InvalidOperationException("Save not Implemented");
        
    protected virtual void DisposeImpl(){}

    protected virtual void ReloadImpl(TContext context, FilePath filePath)
        => throw new InvalidOperationException("Reloading not Supported");
}