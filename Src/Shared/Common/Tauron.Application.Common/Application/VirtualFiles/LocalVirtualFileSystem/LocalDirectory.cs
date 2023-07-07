using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.Resolvers;
using Tauron.ObservableExt;

namespace Tauron.Application.VirtualFiles.LocalVirtualFileSystem;

public class LocalDirectory : DirectoryBase<DirectoryContext>, IHasFileAttributes
{
    private LocalDirectory(DirectoryContext context, FileSystemFeature feature) : base(context, feature) { }
    public override PathInfo OriginalPath => Context.Data.FullName;
    public override Result<DateTime> LastModified => Context.Data.LastWriteTime;

    public static IDirectory New(DirectoryContext context, FileSystemFeature feature)
        => new LocalDirectory(context, feature);
    
    public override Result<IDirectory> ParentDirectory
    {
        get
        {
            if(Context.NoParent) return new NoParentDirectory();

            return 
                from parent in Result.Try(() => Context.Data.Parent)
                select CreateDirectory(parent);

            Result<IDirectory> CreateDirectory(DirectoryInfo? info)
                => info is null ? new NoParentDirectory() : Result.Ok(New(Context with { Data = info }, Features));
        }
    }

    public override bool Exist => Context.Data.Exists;
    public override Result<string> Name => Context.Data.Name;

    public override Result<IEnumerable<IDirectory>> Directories
        => from dics in Result.Try(() => Context.Data.GetDirectories())
            select dics.Select(d => New(Context with { Data = d, NoParent = false }, Features));

    public override Result<IEnumerable<IFile>> Files
        => from files in Result.Try(() => Context.Data.GetFiles())
            select files.Select(f => LocalFile.New(new FileContext(Context.Root, Context.NoParent, f), Features));

    public Result<FileAttributes> Attributes => Result.Try(() => Context.Data.Attributes);

    public Result SetFileAttributes(FileAttributes attributes)
        => Result.Try(new Action(() => Context.Data.Attributes = attributes));

    protected override Result<IDirectory> GetDirectory(DirectoryContext context, PathInfo name)
        => new Errors.NotSupported();

    protected override Result<IFile> GetFile(DirectoryContext context, in PathInfo name)
        => new Errors.NotSupported();

    protected override Result Delete(DirectoryContext context)
        => Result.Try(() => context.Data.Delete(recursive: true));

    protected override Result<IDirectory> RunMoveTo(DirectoryContext context, PathInfo pathInfo) =>
        from validation in ValidateSheme(pathInfo, LocalFileSystemResolver.SchemeName)
        let target = pathInfo.Kind == PathType.Absolute
            ? Path.GetFullPath(pathInfo.Path)
            : Path.GetFullPath(context.Root, pathInfo.Path)
        from move in Result.Try(() => Directory.Move(OriginalPath, target)).ToUnit()
        select New(context with { Data = new DirectoryInfo(target) }, Features);

    protected override Result<IFile> SplitFilePath(PathInfo name) =>
        Result.Try(
            () =>
            {
                string target = Path.Combine(GenericPathHelper.ToRelativePath(OriginalPath), GenericPathHelper.ToRelativePath(name));

                return LocalFile.New(new FileContext(Context.Root, Context.NoParent, new FileInfo(target)), Features);
            });

    protected override Result<IDirectory> SplitDirectoryPath(PathInfo name) =>
        Result.Try(
            () =>
            {
                string target = Path.Combine(GenericPathHelper.ToRelativePath(OriginalPath), GenericPathHelper.ToRelativePath(name));

                return New(Context with { Data = new DirectoryInfo(target), NoParent = false }, Features);
            });
}