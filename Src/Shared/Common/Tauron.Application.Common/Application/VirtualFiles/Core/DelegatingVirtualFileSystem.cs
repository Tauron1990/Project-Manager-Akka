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

    public override DateTime LastModified => Context.LastModified;

    public override IDirectory? ParentDirectory => Context.ParentDirectory;

    public override bool Exist => Context.Exist;

    public override string Name => Context.Name;

    public override IEnumerable<IDirectory> Directories => Context.Directories;

    public override IEnumerable<IFile> Files => Context.Files;

    protected override IDirectory GetDirectory(TContext context, in PathInfo name)
        => context.GetDirectory(name);

    protected override IFile GetFile(TContext context, in PathInfo name)
        => context.GetFile(name);

    protected override void Delete(TContext context)
        => context.Delete();

    protected override IDirectory MovetTo(TContext context, in PathInfo location)
        => context.MoveTo(location);
}