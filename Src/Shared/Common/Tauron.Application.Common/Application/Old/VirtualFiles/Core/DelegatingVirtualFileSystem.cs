using System;
using System.Collections.Generic;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class DelegatingVirtualFileSystem<TContext> : VirtualFileSystemBase<TContext>
    where TContext : IDirectory
{
    protected DelegatingVirtualFileSystem(TContext context, FileSystemFeature feature) : base(context, feature) { }
    protected DelegatingVirtualFileSystem(Func<IFileSystemNode<IDirectory>, TContext> context, FileSystemFeature feature) 
        : base(context, feature) { }

    public override PathInfo OriginalPath => Context.OriginalPath;

    public override DateTime LastModified => Context.LastModified;

    public override IDirectory? ParentDirectory => Context.ParentDirectory;

    public override bool Exist => Context.Exist;

    public override string Name => Context.Name;

    [Pure]
    public override Result<IEnumerable<IDirectory>> Directories() => Context.Directories();

    [Pure]
    public override Result<IEnumerable<IFile>> Files() => Context.Files();

    [Pure]
    protected override Result<IDirectory> GetDirectory(TContext context, in PathInfo name)
        => context.GetDirectory(name);

    [Pure]
    protected override Result<IFile> GetFile(TContext context, in PathInfo name)
        => context.GetFile(name);

    [Pure]
    protected override Result<IDirectory> Delete(TContext context)
        => Create(context.Delete().Cast<TContext>()).Cast<IDirectory>();

    [Pure]
    protected override Result<IDirectory> MovetTo(TContext context, in PathInfo location)
        => context.MoveTo(location);

    [Pure]
    protected abstract Result<DelegatingVirtualFileSystem<TContext>> Create(Result<TContext> context);
}