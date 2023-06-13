using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tauron.Application.VirtualFiles.InMemory.Data;
using Tauron.Errors;

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

    public abstract Result<IEnumerable<IDirectory>> Directories { get; }

    public abstract Result<IEnumerable<IFile>> Files { get; }

    public Result<IFile> GetFile(in PathInfo name)
        => SplitFilePath(name);

    public Result<IDirectory> GetDirectory(in PathInfo name)
        => SplitDirectoryPath(name);

    public Result<IDirectory> MoveTo(PathInfo location) =>
        ValidateFeature(FileSystemFeature.Moveable)
            .Bind(() => RunMoveTo(Context, GenericPathHelper.NormalizePath(location)));

    protected abstract Result<IDirectory> GetDirectory(TContext context, PathInfo name);

    protected abstract Result<IFile> GetFile(TContext context, in PathInfo name);

    private Result<TResult> SplitPath<TResult>(in PathInfo pathInfo, Func<TContext, PathInfo, Result<TResult>> getDirect, Func<TContext, string, string, Result<TResult>> getFromSubpath)
    {
        try
        {
            PathInfo path = GenericPathHelper.NormalizePath(pathInfo);

            if(!path.Path.Contains(GenericPathHelper.GenericSeperator, StringComparison.Ordinal)) return getDirect(Context, path);

            string[] elements = path.Path.Split(GenericPathHelper.GenericSeperator, 2);

            return getFromSubpath(Context, elements[0], elements[1]);
        }
        catch (Exception e)
        {
            return new ExceptionalError(e);
        }
    }

    protected virtual Result<IDirectory> SplitDirectoryPath(in PathInfo name)
    {
        string nameData = name.Path;
        
        return string.IsNullOrEmpty(nameData)
            ? this
            : SplitPath(nameData,
                (context, name1) => GetDirectory(context, name1), 
                (context, path, actualName) => GetDirectory(context, path).Bind(d => d.GetDirectory(actualName)));
    }

    protected virtual Result<IFile> SplitFilePath(in PathInfo name)
    {
        if(string.IsNullOrEmpty(name.Path))
            return new NoFileName();

        return SplitPath(name,
            (context, name1) => GetFile(context, name1),
            (context, path, actualName) => GetDirectory(context, path).Bind(d => d.GetFile(actualName)));
    }

    protected virtual Result<IDirectory> RunMoveTo(TContext directoryContext, PathInfo pathInfo)
        => new NotImplemented(nameof(RunMoveTo));
}