using System;
using System.IO;
using Tauron.Operations;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class VirtualFileSystemBase<TContext> : DirectoryBase<TContext>, IVirtualFileSystem
{
    protected VirtualFileSystemBase(TContext context, FileSystemFeature feature)
        : base(context, feature, NodeType.Root) { }

    protected VirtualFileSystemBase(Func<IFileSystemNode<IDirectory>, TContext> context, FileSystemFeature feature)
        : base(context, feature, NodeType.Root) { }

    public bool IsRealTime => Features.HasFlag(FileSystemFeature.RealTime);

    public bool SaveAfterDispose { get; set; }

    [Pure]
    public IVirtualFileSystem SetSaveAfterDispose(bool value)
    {
        var newSystem = Clone();
        newSystem.SaveAfterDispose = value;
        
        return newSystem;
    }

    public abstract PathInfo Source { get; }

    [Pure]
    public Result<IVirtualFileSystem> Reload(in PathInfo source) =>
        ValidateFeature(
            FileSystemFeature.Reloading,
            (self:this, Context, source), 
            state => state.self.ReloadImpl(state.Context, state.source));

    [Pure]
    public Result<IVirtualFileSystem> Save() =>
        ValidateFeature(
            FileSystemFeature.Save,
            (self:this, Context),
            state => state.self.SaveImpl(state.Context));

    public void Dispose()
    {
        try
        {
            if(Features.HasFlag(FileSystemFeature.Save) && SaveAfterDispose)
#pragma warning disable EPC13
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                Save(); //Dispose
#pragma warning restore EPC13
        }
        finally
        {
            DisposeImpl();
        }
        
        GC.SuppressFinalize(this);
    }

    [Pure]
    protected virtual Result<IVirtualFileSystem> SaveImpl(TContext context)
        => Result.Error<IVirtualFileSystem>(new IOException("Save not Implemented"));

    protected virtual void DisposeImpl() { }

    [Pure]
    protected virtual Result<IVirtualFileSystem> ReloadImpl(TContext context, in PathInfo filePath)
        => Result.Error<IVirtualFileSystem>(new IOException("Reloading not Supported"));

    [Pure]
    protected abstract VirtualFileSystemBase<TContext> Clone();
}