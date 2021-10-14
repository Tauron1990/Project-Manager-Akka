using System;
using System.Collections.Generic;
using System.Reactive.PlatformServices;
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

    private static FileSystemFeature Features
        => FileSystemFeature.Create | FileSystemFeature.Delete | FileSystemFeature.Extension | FileSystemFeature.Moveable |
           FileSystemFeature.Read | FileSystemFeature.Write | FileSystemFeature.RealTime;
    
    private static InMemoryDirectory CreateContext(IFileSystemNode system, ISystemClock clock)
    {
        var root = new InMemoryRoot();
        var dicContext = new DirectoryContext(
            root,
            null,
            root.GetDirectoryEntry("mem::", clock),
            "mem::",
            clock,
            (InMemoryFileSystem)system);

        return new InMemoryDirectory(dicContext, Features);
    }

    public InMemoryFileSystem(ISystemClock clock)
        : base(
            sys => CreateContext(sys, clock),
            Features) { }

    public override FilePath OriginalPath => Context.OriginalPath;

    public override DateTime LastModified => Context.LastModified;

    public override IDirectory? ParentDirectory => Context.ParentDirectory;

    public override bool Exist => Context.Exist;

    public override string Name => Context.Name;
    
    public override IEnumerable<IDirectory> Directories { get; }
    
    public override IEnumerable<IFile> Files { get; }
    protected override IDirectory GetDirectory(InMemoryDirectory context, FilePath name)
        => throw new NotImplementedException();

    protected override IFile GetFile(InMemoryDirectory context, FilePath name)
        => throw new NotImplementedException();

    public override FilePath Source { get; }
}