using System;
using System.Collections.Generic;
using Tauron.Application.Files.Zip.Data;
using Tauron.Application.VirtualFiles;
using Tauron.Application.VirtualFiles.Core;

namespace Tauron.Application.Files.Zip;

public sealed class ZipDirectory : DirectoryBase<ZipDirectoryContext>
{
    public ZipDirectory(ZipDirectoryContext context, FileSystemFeature feature) : base(context, feature) { }
    public override PathInfo OriginalPath { get; }
    public override DateTime LastModified { get; }
    public override IDirectory? ParentDirectory { get; }
    public override bool Exist { get; }
    public override string Name { get; }
    public override IEnumerable<IDirectory> Directories { get; }
    public override IEnumerable<IFile> Files { get; }
    protected override IDirectory GetDirectory(ZipDirectoryContext context, PathInfo name)
        => throw new NotImplementedException();

    protected override IFile GetFile(ZipDirectoryContext context, PathInfo name)
        => throw new NotImplementedException();
}