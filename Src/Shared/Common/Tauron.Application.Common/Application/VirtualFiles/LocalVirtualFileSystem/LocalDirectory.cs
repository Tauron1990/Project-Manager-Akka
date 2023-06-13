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

            Result<DirectoryInfo?> parentResult = Result.Try(() => Context.Data.Parent);

            
            
            return  parentResult.Select<DirectoryInfo?, IDirectory>(parent => parent is null ? new NoParentDirectory() : New(Context with { Data = parent }, Features)));
        }
    }

    public override bool Exist => Context.Data.Exists;
    public override Result<string> Name => Context.Data.Name;

    public override Result<IEnumerable<IDirectory>> Directories
        => from dics in Result.Try(() => Context.Data.GetDirectories())
            select dics.Select(d => New(Context with { Data = d, NoParent = false }, Features));

    public override Result<IEnumerable<IFile>> Files
        => Context.Data.EnumerateFiles().Select(f => new LocalFile(new FileContext(Context.Root, Context.NoParent, f), Features));

    public FileAttributes Attributes
    {
        get => Context.Data.Attributes;
        set => Context.Data.Attributes = value;
    }

    Result SetFileAttributes(FileAttributes attributes);
    
    protected override Result<IDirectory> GetDirectory(DirectoryContext context, PathInfo name)
        => throw new NotSupportedException("Never called Method");

    protected override Result<IFile> GetFile(DirectoryContext context, in PathInfo name)
        => throw new NotSupportedException("Never called Method");

    protected override void Delete(DirectoryContext context)
        => context.Data.Delete(true);

    protected override Result<IDirectory> RunMoveTo(DirectoryContext context, in PathInfo location)
    {
        ValidateSheme(location, LocalFileSystemResolver.SchemeName);

        string target = location.Kind == PathType.Absolute
            ? Path.GetFullPath(location.Path)
            : Path.GetFullPath(context.Root, location.Path);
        Directory.Move(OriginalPath, target);

        return new LocalDirectory(context with { Data = new DirectoryInfo(target) }, Features);
    }

    protected override Result<IFile> SplitFilePath(in PathInfo name)
    {
        string target = Path.Combine(GenericPathHelper.ToRelativePath(OriginalPath), GenericPathHelper.ToRelativePath(name));

        return new LocalFile(new FileContext(Context.Root, Context.NoParent, new FileInfo(target)), Features);
    }

    protected override Result<IDirectory> SplitDirectoryPath(in PathInfo name)
    {
        string target = Path.Combine(GenericPathHelper.ToRelativePath(OriginalPath), GenericPathHelper.ToRelativePath(name));

        return new LocalDirectory(Context with { Data = new DirectoryInfo(target), NoParent = false }, Features);
    }
}