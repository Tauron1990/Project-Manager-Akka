using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class VirtualFileSystemBase<TContext> : DirectoryBase<TContext>, IVirtualFileSystem
{
    protected VirtualFileSystemBase(TContext context, FileSystemFeature feature) 
        : base(context, feature, NodeType.Root) { }

    protected VirtualFileSystemBase(Func<IFileSystemNode, TContext> context, FileSystemFeature feature) 
        : base(context, feature, NodeType.Root) { }
    
    public bool IsRealTime => Features.HasFlag(FileSystemFeature.RealTime);
        
    public bool SaveAfterDispose { get; set; }
        
    public abstract PathInfo Source { get; }
        
    public void Reload(PathInfo source)
    {
        ValidateFeature(FileSystemFeature.Reloading);
        ReloadImpl(Context, source);
    }

    public void Save()
    {
        ValidateFeature(FileSystemFeature.Save);
            
        SaveImpl(Context);
    }

    public void Dispose()
    {
        try
        {
            if (Features.HasFlag(FileSystemFeature.Save) && SaveAfterDispose)
                Save();
        }
        finally
        {
            DisposeImpl();
        }
    }

    protected virtual void SaveImpl(TContext context)
        => throw new InvalidOperationException("Save not Implemented");
        
    protected virtual void DisposeImpl(){}

    protected virtual void ReloadImpl(TContext context, PathInfo filePath)
        => throw new InvalidOperationException("Reloading not Supported");
}

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
    protected override IDirectory GetDirectory(TContext context, PathInfo name)
        => context.GetDirectory(name);

    protected override IFile GetFile(TContext context, PathInfo name)
        => context.GetFile(name);

    protected override void Delete(TContext context)
        => context.Delete();

    protected override IDirectory MovetTo(TContext context, PathInfo location)
        => context.MoveTo(location);
}