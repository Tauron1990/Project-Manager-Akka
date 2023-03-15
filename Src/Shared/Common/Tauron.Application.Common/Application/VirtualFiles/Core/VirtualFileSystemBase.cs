using System;
using JetBrains.Annotations;
using Tauron.Operations;

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

    public abstract PathInfo Source { get; }

    public SimpleResult Reload(in PathInfo source) =>
        ValidateFeature(
            FileSystemFeature.Reloading,
            (self:this, Context, source), 
            state => state.self.ReloadImpl(state.Context, state.source));

    public SimpleResult Save() =>
        ValidateFeature(
            FileSystemFeature.Save,
            (self:this, Context),
            state => state.self.SaveImpl(state.Context));

    public void Dispose()
    {
        try
        {
            if(Features.HasFlag(FileSystemFeature.Save) && SaveAfterDispose)
                Save();
        }
        finally
        {
            DisposeImpl();
        }
        
        GC.SuppressFinalize(this);
    }

    protected virtual SimpleResult SaveImpl(TContext context)
        => SimpleResult.Failure("Save not Implemented");

    protected virtual void DisposeImpl() { }

    protected virtual SimpleResult ReloadImpl(TContext context, in PathInfo filePath)
        => SimpleResult.Failure("Reloading not Supported");
}