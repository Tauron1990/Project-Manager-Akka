using System;
using System.Reactive.PlatformServices;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;
using Tauron.ObservableExt;

namespace Tauron.Application.VirtualFiles.InMemory;

public sealed class InMemoryFileSystem : DelegatingVirtualFileSystem<InMemoryDirectory>
{
    public InMemoryFileSystem(ISystemClock clock, PathInfo start)
        : base(
            sys => CreateContext(sys, start, clock).Value,
            ReadyFeatures & ~(FileSystemFeature.Moveable | FileSystemFeature.Delete)) { }

    private static FileSystemFeature ReadyFeatures
        => FileSystemFeature.Create | FileSystemFeature.Delete | FileSystemFeature.Extension | FileSystemFeature.Moveable |
           FileSystemFeature.Read | FileSystemFeature.Write | FileSystemFeature.RealTime;

    public override PathInfo Source => Context.OriginalPath;

    internal Result<TResult> MoveElement<TResult, TElement>(string name, in PathInfo path, TElement element, Func<DirectoryContext, PathInfo, TElement, TResult> factory)
        where TElement : IDataElement
        where TResult : class =>
        from tempDic in GetDirectory(GenericPathHelper.ToRelativePath(path))
        let dic = tempDic == this
            ? Context
            : (InMemoryDirectory)tempDic
        let actualElement = element.SetName(name)
        from addOk in dic.TryAddElement(name, actualElement)
        select addOk
            ? factory(dic.DirectoryContext, GenericPathHelper.Combine(dic.OriginalPath, name), (TElement)actualElement)
            : null;

#pragma warning disable EPS05
    private static Result<InMemoryDirectory> CreateContext(IFileSystemNode system, PathInfo startPath, ISystemClock clock)
#pragma warning restore EPS05
    {
        var root = new InMemoryRoot();

        return
            from entry in root.GetDirectoryEntry(startPath, clock)
            let context = new DirectoryContext(root, null!, entry, startPath, clock, (InMemoryFileSystem)system)
            select (InMemoryDirectory)InMemoryDirectory.New(context, ReadyFeatures);
    }

    protected override void DisposeImpl()
    {
#pragma warning disable EPC13
        Context.Delete().LogIfFailed();
#pragma warning restore EPC13
        Context.DirectoryContext.Root.Dispose();
    }
}