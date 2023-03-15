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

    public abstract Result<IEnumerable<IDirectory>> Directories();

    public abstract Result<IEnumerable<IFile>> Files();

    public Result<IFile> GetFile(in PathInfo name)
        => Result.FromFunc((name, Self:this), static parms =>  parms.Self.SplitFilePath(parms.name));

    public Result<IDirectory> GetDirectory(in PathInfo name)
        => SplitDirectoryPath(name);

    public Result<IDirectory> MoveTo(in PathInfo location) =>
        ValidateFeature(
            FileSystemFeature.Moveable,
            (Context, location, Self:this), 
            state => state.Self.MovetTo(state.Context, state.location));

    protected abstract Result<IDirectory> GetDirectory(TContext context, in PathInfo name);

    protected abstract Result<IFile> GetFile(TContext context, in PathInfo name);

    private TResult SplitPath<TResult>(in PathInfo pathInfo, Func<TContext, PathInfo, TResult> getDirect, Func<TContext, string, string, TResult> getFromSubpath)
    {
        PathInfo path = GenericPathHelper.NormalizePath(pathInfo);

        if(!path.Path.Contains(GenericPathHelper.GenericSeperator, StringComparison.Ordinal)) return getDirect(Context, path);

        string[] elements = path.Path.Split(GenericPathHelper.GenericSeperator, 2);

        return getFromSubpath(Context, elements[0], elements[1]);
    }

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

    protected virtual Result<IFile> SplitFilePath(in PathInfo name)
    {
        if(name.Path.IsNullOrEmpty())
            return Result.Error<IFile>(new InvalidOperationException("No File Name Provided"));

        return SplitPath<Result<IFile>>(
                name,
                (context, name1) => GetFile(context, name1),
                (context, path, actualName) => GetDirectory(context, path).Select(dic => dic.GetFile(actualName)));
    }

    protected virtual Result<IDirectory> MovetTo(TContext context, in PathInfo location)
        => Result.Error<IDirectory>(new IOException("Move is not Implemented"));
}