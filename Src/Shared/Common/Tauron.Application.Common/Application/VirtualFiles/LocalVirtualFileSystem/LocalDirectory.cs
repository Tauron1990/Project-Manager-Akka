using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stl;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.Resolvers;

namespace Tauron.Application.VirtualFiles.LocalVirtualFileSystem;

public class LocalDirectory : DirectoryBase<DirectoryContext>, IHasFileAttributes
{
    public LocalDirectory(DirectoryContext context, FileSystemFeature feature) : base(context, feature) { }
    public override PathInfo OriginalPath => Context.Data.FullName;
    public override FluentResults.Result<DateTime> LastModified => Context.Data.LastWriteTime;

    public override Result<IDirectory> ParentDirectory
    {
        get
        {
            if(Context.NoParent) return null;

            DirectoryInfo? parent = Context.Data.Parent;

            return parent is null ? null : new LocalDirectory(Context with { Data = parent }, Features);
        }
    }

    public override bool Exist => Context.Data.Exists;
    public override FluentResults.Result<string> Name => Context.Data.Name;

    public override FluentResults.Result<IEnumerable<IDirectory>> Directories
        => Context.Data.EnumerateDirectories().Select(d => new LocalDirectory(Context with { Data = d, NoParent = false }, Features));

    public override FluentResults.Result<IEnumerable<IFile>> Files
        => Context.Data.EnumerateFiles().Select(f => new LocalFile(new FileContext(Context.Root, Context.NoParent, f), Features));

    public FileAttributes Attributes
    {
        get => Context.Data.Attributes;
        set => Context.Data.Attributes = value;
    }

    protected override FluentResults.Result<IDirectory> GetDirectory(DirectoryContext context, in PathInfo name)
        => throw new NotSupportedException("Never called Method");

    protected override FluentResults.Result<IFile> GetFile(DirectoryContext context, in PathInfo name)
        => throw new NotSupportedException("Never called Method");

    protected override void Delete(DirectoryContext context)
        => context.Data.Delete(true);

    protected override FluentResults.Result<IDirectory> RunMoveTo(DirectoryContext context, in PathInfo location)
    {
        ValidateSheme(location, LocalFileSystemResolver.SchemeName);

        string target = location.Kind == PathType.Absolute
            ? Path.GetFullPath(location.Path)
            : Path.GetFullPath(context.Root, location.Path);
        Directory.Move(OriginalPath, target);

        return new LocalDirectory(context with { Data = new DirectoryInfo(target) }, Features);
    }

    protected override FluentResults.Result<IFile> SplitFilePath(in PathInfo name)
    {
        string target = Path.Combine(GenericPathHelper.ToRelativePath(OriginalPath), GenericPathHelper.ToRelativePath(name));

        return new LocalFile(new FileContext(Context.Root, Context.NoParent, new FileInfo(target)), Features);
    }

    protected override FluentResults.Result<IDirectory> SplitDirectoryPath(in PathInfo name)
    {
        string target = Path.Combine(GenericPathHelper.ToRelativePath(OriginalPath), GenericPathHelper.ToRelativePath(name));

        return new LocalDirectory(Context with { Data = new DirectoryInfo(target), NoParent = false }, Features);
    }
}