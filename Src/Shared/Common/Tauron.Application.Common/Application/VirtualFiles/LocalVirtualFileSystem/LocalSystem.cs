using System.IO;
using Tauron.Application.VirtualFiles.Core;

namespace Tauron.Application.VirtualFiles.LocalVirtualFileSystem;

public sealed class LocalSystem : DelegatingVirtualFileSystem<LocalDirectory>, IHasFileAttributes
{
    private string _rootPath;

    public LocalSystem(string rootPath)
        : base(new LocalDirectory(new DirectoryContext(rootPath, NoParent: true, new DirectoryInfo(rootPath)), LocalFeaturs), LocalFeaturs)
        => _rootPath = rootPath;

    private static FileSystemFeature LocalFeaturs => FileSystemFeature.Create | FileSystemFeature.Delete | FileSystemFeature.Extension | FileSystemFeature.Moveable
                                                   | FileSystemFeature.Read | FileSystemFeature.Reloading | FileSystemFeature.Write | FileSystemFeature.RealTime;

    public override PathInfo Source => _rootPath;

    public Result<FileAttributes> Attributes
    {
        get => Context.Attributes;
        set => Context.Attributes = value;
    }

    protected override void ReloadImpl(LocalDirectory context, in PathInfo filePath)
    {
        _rootPath = GenericPathHelper.ToRelativePath(filePath);
        Context = new LocalDirectory(new DirectoryContext(_rootPath, true, new DirectoryInfo(_rootPath)), LocalFeaturs);
    }
}