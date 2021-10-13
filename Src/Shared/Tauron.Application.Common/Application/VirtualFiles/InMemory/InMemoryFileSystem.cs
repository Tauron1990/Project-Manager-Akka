using System;
using System.Collections.Generic;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;

namespace Tauron.Application.VirtualFiles.InMemory;

public class InMemoryFileSystem : VirtualFileSystemBase<InMemoryDirectory>
{

    internal TResult? MoveElement<TResult, TElement>(string name, FilePath path, TElement element, Func<DirectoryContext, FilePath, TElement, TResult> factory)
        where TElement : IDataElement
        where TResult : class
    {
        var relative = GenericPathHelper.ToRelativePath(path);
        var dic = (InMemoryDirectory)GetDirectory(relative);

        return dic.TryAddElement(name, element) ? factory(dic.DirectoryContext, relative, element) : null;
    }

    public InMemoryFileSystem(InMemoryDirectory context, FileSystemFeature feature) : base(context, feature) { }
    public override FilePath OriginalPath { get; }
    public override DateTime LastModified { get; }
    public override IDirectory? ParentDirectory { get; }
    public override bool Exist { get; }
    public override string Name { get; }
    public override IEnumerable<IDirectory> Directories { get; }
    public override IEnumerable<IFile> Files { get; }
    protected override IDirectory GetDirectory(InMemoryDirectory context, FilePath name)
        => throw new NotImplementedException();

    protected override IFile GetFile(InMemoryDirectory context, FilePath name)
        => throw new NotImplementedException();

    public override FilePath Source { get; }
}