using System;
using System.Collections.Generic;
using System.IO;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class DirectoryBase<TContext> : SystemNodeBase<IDirectory, TContext>, IDirectory 
{
    protected DirectoryBase(TContext context, FileSystemFeature feature, NodeType nodeType)
        : base(context, feature, nodeType) { }

    protected DirectoryBase(TContext context, FileSystemFeature feature)
        : base(context, feature, NodeType.Directory) { }

    protected DirectoryBase(Func<IFileSystemNode<IDirectory>, TContext> context, FileSystemFeature feature, NodeType nodeType)
        : base(context, feature, nodeType) { }

    protected DirectoryBase(Func<IFileSystemNode<IDirectory>, TContext> context, FileSystemFeature feature)
        : base(context, feature, NodeType.Directory) { }

    [Pure]
    public abstract Result<IEnumerable<IDirectory>> Directories();

    [Pure]
    public abstract Result<IEnumerable<IFile>> Files();

    [Pure]
    public Result<IFile> GetFile(in PathInfo name)
        => Result.FromFunc((name, Self:this), static parms =>  parms.Self.SplitFilePath(parms.name));

    [Pure]
    public Result<IDirectory> GetDirectory(in PathInfo name)
        => SplitDirectoryPath(name);

    [Pure]
    public Result<IDirectory> MoveTo(in PathInfo location) =>
        ValidateFeature(
            FileSystemFeature.Moveable,
            (Context, location, Self:this), 
            state => state.Self.MovetTo(state.Context, state.location));

    [Pure]
    protected abstract Result<IDirectory> GetDirectory(TContext context, in PathInfo name);

    [Pure]
    protected abstract Result<IFile> GetFile(TContext context, in PathInfo name);

    [Pure]
    private TResult SplitPath<TResult>(in PathInfo pathInfo, Func<TContext, PathInfo, TResult> getDirect, Func<TContext, string, string, TResult> getFromSubpath)
    {
        PathInfo path = GenericPathHelper.NormalizePath(pathInfo);

        if(!path.Path.Contains(GenericPathHelper.GenericSeperator, StringComparison.Ordinal)) return getDirect(Context, path);

        string[] elements = path.Path.Split(GenericPathHelper.GenericSeperator, 2);

        return getFromSubpath(Context, elements[0], elements[1]);
    }

    [Pure]
    protected virtual Result<IDirectory> SplitDirectoryPath(in PathInfo name)
    {
        string nameData = name.Path;

        return nameData.IsNullOrEmpty()
            ? this
            : SplitPath<Result<IDirectory>>(
                nameData,
                (context, name1) => GetDirectory(context, name1),
                (context, path, actualName) => GetDirectory(context, path).Select(dic => dic.GetDirectory(actualName)));
    }

    [Pure]
    protected virtual Result<IFile> SplitFilePath(in PathInfo name)
    {
        if(name.Path.IsNullOrEmpty())
            return Result.Error<IFile>(new InvalidOperationException("No File Name Provided"));

        return SplitPath<Result<IFile>>(
                name,
                (context, name1) => GetFile(context, name1),
                (context, path, actualName) => GetDirectory(context, path).Select(dic => dic.GetFile(actualName)));
    }

    [Pure]
    protected virtual Result<IDirectory> MovetTo(TContext context, in PathInfo location)
        => Result.Error<IDirectory>(new IOException("Move is not Implemented"));
}