using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class DelegatingVirtualFileSystem<TContext> : VirtualFileSystemBase<TContext>
    where TContext : IDirectory
{
    protected DelegatingVirtualFileSystem(TContext context, FileSystemFeature feature) : base(context, feature) { }
    protected DelegatingVirtualFileSystem(Func<IFileSystemNode, TContext> context, FileSystemFeature feature) : base(context, feature) { }

    public override PathInfo OriginalPath => Context.OriginalPath;

    public override Result<DateTime> LastModified => Context.LastModified;

    public override Result<IDirectory> ParentDirectory => Context.ParentDirectory;

    public override bool Exist => Context.Exist;

    public override Result<string> Name => Context.Name;

    public override Result<IEnumerable<IDirectory>> Directories => Context.Directories;

    public override Result<IEnumerable<IFile>> Files => Context.Files;

    protected override Result<IDirectory> GetDirectory(TContext context, PathInfo name)
        => context.GetDirectory(name);

    protected override Result<IFile> GetFile(TContext context, in PathInfo name)
        => context.GetFile(name);

    protected override Result Delete(TContext context)
        => context.Delete();

    protected override Result<IDirectory> RunMoveTo(TContext context, PathInfo location)
        => context.MoveTo(location);
}