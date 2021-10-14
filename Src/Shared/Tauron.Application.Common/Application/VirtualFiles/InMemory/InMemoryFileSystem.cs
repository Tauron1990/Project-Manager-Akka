using System;
using System.Reactive.PlatformServices;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;

namespace Tauron.Application.VirtualFiles.InMemory;

public sealed class InMemoryFileSystem : DelegatingVirtualFileSystem<InMemoryDirectory>
{

    internal TResult? MoveElement<TResult, TElement>(string name, PathInfo path, TElement element, Func<DirectoryContext, PathInfo, TElement, TResult> factory)
        where TElement : IDataElement
        where TResult : class
    {
        var relative = GenericPathHelper.ToRelativePath(path);
        var dic = (InMemoryDirectory)GetDirectory(relative);

        return dic.TryAddElement(name, element) ? factory(dic.DirectoryContext, relative, element) : null;
    }

    private static FileSystemFeature ReadyFeatures
        => FileSystemFeature.Create | FileSystemFeature.Delete | FileSystemFeature.Extension | FileSystemFeature.Moveable |
           FileSystemFeature.Read | FileSystemFeature.Write | FileSystemFeature.RealTime;
    
    private static InMemoryDirectory CreateContext(IFileSystemNode system, PathInfo startPath, ISystemClock clock)
    {
        var root = new InMemoryRoot();
        var dicContext = new DirectoryContext(
            root,
            null,
            root.GetDirectoryEntry(startPath, clock),
            startPath,
            clock,
            (InMemoryFileSystem)system);

        return new InMemoryDirectory(dicContext, ReadyFeatures);
    }

    public InMemoryFileSystem(ISystemClock clock, PathInfo start)
        : base(
            sys => CreateContext(sys, start, clock),
            ReadyFeatures & ~(FileSystemFeature.Moveable | FileSystemFeature.Delete)) { }

    public override PathInfo OriginalPath => Name;

    public override PathInfo Source => Context.OriginalPath;

    protected override void DisposeImpl()
    {
        Context.Delete();
        Context.DirectoryContext.Root.Dispose();
    }
}