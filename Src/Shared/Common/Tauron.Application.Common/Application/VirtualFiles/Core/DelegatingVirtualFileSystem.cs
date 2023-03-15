using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Stl;
using Tauron.Operations;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class DelegatingVirtualFileSystem<TContext> : VirtualFileSystemBase<TContext>
    where TContext : IDirectory
{
    protected DelegatingVirtualFileSystem(TContext context, FileSystemFeature feature) : base(context, feature) { }
    protected DelegatingVirtualFileSystem(Func<IFileSystemNode, TContext> context, FileSystemFeature feature) : base(context, feature) { }

    public override PathInfo OriginalPath => Context.OriginalPath;

    public override DateTime LastModified => Context.LastModified;

    public override IDirectory? ParentDirectory => Context.ParentDirectory;

    public override bool Exist => Context.Exist;

    public override string Name => Context.Name;

    public override Result<IEnumerable<IDirectory>> Directories() => Context.Directories();

    public override Result<IEnumerable<IFile>> Files() => Context.Files();

    protected override Result<IDirectory> GetDirectory(TContext context, in PathInfo name)
        => context.GetDirectory(name);

    protected override Result<IFile> GetFile(TContext context, in PathInfo name)
        => context.GetFile(name);

    protected override SimpleResult Delete(TContext context)
        => context.Delete();

    protected override Result<IDirectory> MovetTo(TContext context, in PathInfo location)
        => context.MoveTo(location);
}