using System;
using System.Collections.Generic;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;

namespace Tauron.Application.VirtualFiles.InMemory;

public class InMemoryFileSystem : VirtualFileSystemBase<DirectoryContext>
{
    public InMemoryFileSystem(DirectoryContext context, FileSystemFeature feature) : base(context, feature) { }
    public override FilePath OriginalPath { get; }
    public override DateTime LastModified { get; }
    public override IDirectory? ParentDirectory { get; }
    public override bool Exist { get; }
    public override string Name { get; }
    public override IEnumerable<IDirectory> Directories { get; }
    public override IEnumerable<IFile> Files { get; }
    protected override IDirectory GetDirectory(DirectoryContext context, FilePath name)
        => throw new NotImplementedException();

    protected override IFile GetFile(DirectoryContext context, FilePath name)
        => throw new NotImplementedException();

    public override FilePath Source { get; }
    protected override void SaveImpl(DirectoryContext context)
    {
        throw new NotImplementedException();
    }

    internal TResult? MoveElement<TResult, TElement>(string name, FilePath path, TElement element, Func<DirectoryContext, TElement, TResult> factory)
        where TElement : IDataElement
        where TResult : class
    {
        var dic = (InMemoryDirectory)GetDirectory(path);

        return dic.TryAddElement(name, element) ? factory(dic.DirectoryContext, element) : null;
    }

    protected override void DisposeImpl()
    {
        throw new NotImplementedException();
    }

}