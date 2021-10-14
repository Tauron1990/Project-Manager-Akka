using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class DirectoryBase<TContext> : SystemNodeBase<TContext>, IDirectory
{
    protected DirectoryBase(TContext context, FileSystemFeature feature, NodeType nodeType) 
        : base(context, feature, nodeType) { }
        
    protected DirectoryBase(TContext context, FileSystemFeature feature) 
        : base(context, feature, NodeType.Directory) { }

    protected DirectoryBase(Func<IFileSystemNode, TContext> context, FileSystemFeature feature, NodeType nodeType) 
        : base(context, feature, nodeType) { }
        
    protected DirectoryBase(Func<IFileSystemNode, TContext> context, FileSystemFeature feature) 
        : base(context, feature, NodeType.Directory) { }
    
    public abstract IEnumerable<IDirectory> Directories { get; }
        
    public abstract IEnumerable<IFile> Files { get; }

    protected abstract IDirectory GetDirectory(TContext context, FilePath name);
        
    protected abstract IFile GetFile(TContext context, FilePath name);

    private TResult SplitPath<TResult>(FilePath path, Func<TContext, FilePath, TResult> getDirect, Func<TContext, string, string, TResult> getFromSubpath)
    {
        path = GenericPathHelper.NormalizePath(path);

        if (!path.Path.Contains(GenericPathHelper.GenericSeperator)) return getDirect(Context, path);

        var elements = path.Path.Split(GenericPathHelper.GenericSeperator, 2);

        return getFromSubpath(Context, elements[0], elements[1]);
    }

    protected virtual IDirectory SplitDirectoryPath(FilePath name)
        => SplitPath(name.Path, GetDirectory, (context, path, actualName) => GetDirectory(context, path).GetDirectory(actualName));

    protected virtual IFile SplitFilePath(FilePath name)
        => SplitPath(name, GetFile, (context, path, actualName) => GetDirectory(context, path).GetFile(actualName));

    public IFile GetFile(FilePath name)
        => SplitFilePath(name);

    public IDirectory GetDirectory(FilePath name)
        => SplitDirectoryPath(name);

    public IDirectory MoveTo(FilePath location)
    {
        ValidateFeature(FileSystemFeature.Moveable);
            
        return MovetTo(Context, GenericPathHelper.NormalizePath(location));
    }

    protected virtual IDirectory MovetTo(TContext context, FilePath location)
        => throw new IOException("Move is not Implemented");
}