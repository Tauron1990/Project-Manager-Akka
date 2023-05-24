using System;
using System.Reactive.PlatformServices;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;

namespace Tauron.Application.VirtualFiles.InMemory;

public sealed class InMemoryFileSystem : DelegatingVirtualFileSystem<InMemoryDirectory>
{
    public InMemoryFileSystem(ISystemClock clock, PathInfo start)
        : base(
            sys => CreateContext(sys, start, clock),
            ReadyFeatures & ~(FileSystemFeature.Moveable | FileSystemFeature.Delete)) { }

    private static FileSystemFeature ReadyFeatures
        => FileSystemFeature.Create | FileSystemFeature.Delete | FileSystemFeature.Extension | FileSystemFeature.Moveable |
           FileSystemFeature.Read | FileSystemFeature.Write | FileSystemFeature.RealTime;

    public override PathInfo Source => Context.OriginalPath;

    internal Result<TResult> MoveElement<TResult, TElement>(string name, in PathInfo path, TElement element, Func<DirectoryContext, PathInfo, TElement, TResult> factory)
        where TElement : IDataElement
        where TResult : class
    {
        PathInfo relative = GenericPathHelper.ToRelativePath(path);
        IDirectory tempdic = GetDirectory(relative);
        InMemoryDirectory dic;

        if(tempdic == this)
            dic = Context;
        else
            dic = (InMemoryDirectory)tempdic;

        element.Name = name;

        return dic.TryAddElement(name, element)
            ? factory(dic.DirectoryContext, GenericPathHelper.Combine(dic.OriginalPath, name), element)
            : null;
    }

    private static InMemoryDirectory CreateContext(IFileSystemNode system, in PathInfo startPath, ISystemClock clock)
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

    protected override void DisposeImpl()
    {
        Context.Delete();
        Context.DirectoryContext.Root.Dispose();
    }
}