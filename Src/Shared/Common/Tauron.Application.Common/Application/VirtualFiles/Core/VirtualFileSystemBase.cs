using System;
using JetBrains.Annotations;
using Tauron.Errors;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class VirtualFileSystemBase<TContext> : DirectoryBase<TContext>, IVirtualFileSystem
{
    protected VirtualFileSystemBase(TContext context, FileSystemFeature feature)
        : base(context, feature, NodeType.Root) { }

    protected VirtualFileSystemBase(Func<IFileSystemNode, TContext> context, FileSystemFeature feature)
        : base(context, feature, NodeType.Root) { }

    public bool IsRealTime => Features.HasFlag(FileSystemFeature.RealTime);

    public bool SaveWhenDispose { get; set; }

    public abstract PathInfo Source { get; }

    public Result Reload(PathInfo source) =>
        ValidateFeature(FileSystemFeature.Reloading)
            .Bind(() => ReloadImpl(Context, source));

    public Result Save() =>
        ValidateFeature(FileSystemFeature.Save)
            .Bind(() => SaveImpl(Context));

    public void Dispose()
    {
        try
        {
            if(Features.HasFlag(FileSystemFeature.Save) && SaveWhenDispose)
            #pragma warning disable EPC13
                Save().LogIfFailed<VirtualFileSystemBase<TContext>>();
        }
        finally
        {
            DisposeImpl();
        }
    }

    protected virtual Result SaveImpl(TContext context)
        => new NotImplemented(nameof(SaveImpl));

    protected virtual void DisposeImpl() { }

    protected virtual Result ReloadImpl(TContext context, in PathInfo filePath)
        => new NotImplemented(nameof(ReloadImpl));
}