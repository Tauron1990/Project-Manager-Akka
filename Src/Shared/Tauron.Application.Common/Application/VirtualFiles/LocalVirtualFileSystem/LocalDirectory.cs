using System;
using System.Collections.Generic;
using Tauron.Application.VirtualFiles.Core;

namespace Tauron.Application.VirtualFiles.LocalVirtualFileSystem;

public class LocalDirectory : DirectoryBase<DirectoryContext>
{
    public LocalDirectory(DirectoryContext context, FileSystemFeature feature) : base(context, feature) { }
    public override PathInfo OriginalPath => Context.Data.FullName;
    public override DateTime LastModified => Context.Data.LastWriteTime;

    public override IDirectory? ParentDirectory
    {
        get
        {
            if (Context.NoParent) return null;

            var parent = Context.Data.Parent;

            return parent is null ? null : new LocalDirectory(Context with { Data = parent }, Features);
        }
    }

    public override bool Exist => Context.Data.Exists;
    public override string Name => Context.Data.Name;
    public override IEnumerable<IDirectory> Directories { get; }
    public override IEnumerable<IFile> Files { get; }
    protected override IDirectory GetDirectory(DirectoryContext context, PathInfo name)
        => throw new NotImplementedException();

    protected override IFile GetFile(DirectoryContext context, PathInfo name)
        => throw new NotImplementedException();
}