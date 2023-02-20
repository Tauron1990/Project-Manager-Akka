using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Stl;

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

    public IFile GetFile(in PathInfo name)
        => SplitFilePath(name);

    public IDirectory GetDirectory(in PathInfo name)
        => SplitDirectoryPath(name);

    public IDirectory MoveTo(in PathInfo location)
    {
        ValidateFeature(FileSystemFeature.Moveable);

        return MovetTo(Context, GenericPathHelper.NormalizePath(location));
    }

    protected abstract IDirectory GetDirectory(TContext context, in PathInfo name);

    protected abstract IFile GetFile(TContext context, in PathInfo name);

    private TResult SplitPath<TResult>(in PathInfo pathInfo, Func<TContext, PathInfo, TResult> getDirect, Func<TContext, string, string, TResult> getFromSubpath)
    {
        PathInfo path = GenericPathHelper.NormalizePath(pathInfo);

        if(!path.Path.Contains(GenericPathHelper.GenericSeperator, StringComparison.Ordinal)) return getDirect(Context, path);

        string[] elements = path.Path.Split(GenericPathHelper.GenericSeperator, 2);

        return getFromSubpath(Context, elements[0], elements[1]);
    }

    protected virtual IDirectory SplitDirectoryPath(in PathInfo name)
    {
        string nameData = name.Path;

        return nameData.IsNullOrEmpty()
            ? this
            : SplitPath(nameData, (context, name1) => GetDirectory(context, name1), (context, path, actualName) => GetDirectory(context, path).GetDirectory(actualName));
    }

    protected virtual IFile SplitFilePath(in PathInfo name)
    {
        if(name.Path.IsNullOrEmpty())
            throw new InvalidOperationException("No File Name Provided");

        return SplitPath(name, (context, name1) => GetFile(context, name1), (context, path, actualName) => GetDirectory(context, path).GetFile(actualName));
    }

    protected virtual IDirectory MovetTo(TContext context, in PathInfo location)
        => throw new IOException("Move is not Implemented");
}