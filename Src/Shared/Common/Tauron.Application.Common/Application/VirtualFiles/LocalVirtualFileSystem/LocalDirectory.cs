using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.Resolvers;

namespace Tauron.Application.VirtualFiles.LocalVirtualFileSystem;

public class LocalDirectory : DirectoryBase<DirectoryContext>, IHasFileAttributes
{
    public LocalDirectory(DirectoryContext context, FileSystemFeature feature) : base(context, feature) { }
    public override PathInfo OriginalPath => Context.Data.FullName;
    public override DateTime LastModified => Context.Data.LastWriteTime;

    public override IDirectory? ParentDirectory
    {
        get
        {
            if (Context.NoParent) return null;

            var parent = Context.Data.Parent;

            return parent is null ? null : new LocalDirectory(Context with { Data = parent }, Features);
        }
    }

    public override bool Exist => Context.Data.Exists;
    public override string Name => Context.Data.Name;
    public override IEnumerable<IDirectory> Directories 
        => Context.Data.EnumerateDirectories().Select(d => new LocalDirectory(Context with { Data = d, NoParent = false }, Features));

    public override IEnumerable<IFile> Files
        => Context.Data.EnumerateFiles().Select(f => new LocalFile(new FileContext(Context.Root, Context.NoParent, f), Features));
    protected override IDirectory GetDirectory(DirectoryContext context, PathInfo name)
        => throw new NotSupportedException("Never called Method");

    protected override IFile GetFile(DirectoryContext context, PathInfo name)
        => throw new NotSupportedException("Never called Method");

    protected override void Delete(DirectoryContext context)
        => context.Data.Delete(recursive: true);

    protected override IDirectory MovetTo(DirectoryContext context, PathInfo location)
    {
        ValidateSheme(location, LocalFileSystemResolver.SchemeName);

        var target = location.Kind == PathType.Absolute 
            ? Path.GetFullPath(location.Path)
            : Path.GetFullPath(context.Root, location.Path);
        Directory.Move(OriginalPath, target);

        return new LocalDirectory(context with { Data = new DirectoryInfo(target) }, Features);
    }

    protected override IFile SplitFilePath(PathInfo name)
    {
        var target = Path.Combine(GenericPathHelper.ToRelativePath(OriginalPath), GenericPathHelper.ToRelativePath(name));

        return new LocalFile(new FileContext(Context.Root, Context.NoParent, new FileInfo(target)), Features);
    }

    protected override IDirectory SplitDirectoryPath(PathInfo name)
    {
        var target = Path.Combine(GenericPathHelper.ToRelativePath(OriginalPath), GenericPathHelper.ToRelativePath(name));

        return new LocalDirectory(Context with { Data = new DirectoryInfo(target), NoParent = false }, Features);
    }

    public FileAttributes Attributes
    {
        get => Context.Data.Attributes;
        set => Context.Data.Attributes = value;
    }
}