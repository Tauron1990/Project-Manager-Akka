using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using Stl;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.Resolvers;
using Tauron.Operations;

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
            if(Context.NoParent) return null;

            DirectoryInfo? parent = Context.Data.Parent;

            return parent is null ? null : new LocalDirectory(Context with { Data = parent }, Features);
        }
    }

    public override bool Exist => Context.Data.Exists;
    public override string Name => Context.Data.Name;

    public override Result<IEnumerable<IDirectory>> Directories() =>
        Result.FromFunc(
            this,
            static self => self.Context.Data.GetDirectories()
                .Select(d => (IDirectory)new LocalDirectory(self.Context with { Data = d, NoParent = false }, self.Features)));

    public override Result<IEnumerable<IFile>> Files() =>
        Result.FromFunc(
            this,
            static self => self.Context.Data.GetFiles()
                .Select(f => (IFile)new LocalFile(new FileContext(self.Context.Root, self.Context.NoParent, f), self.Features)));

    public FileAttributes Attributes
    {
        get => Context.Data.Attributes;
        set => Context.Data.Attributes = value;
    }

    protected override Result<IDirectory> GetDirectory(DirectoryContext context, in PathInfo name)
        => Result.Error<IDirectory>(new NotSupportedException("Never called Method"));

    protected override Result<IFile> GetFile(DirectoryContext context, in PathInfo name)
        => Result.Error<IFile>(new NotSupportedException("Never called Method"));

    protected override SimpleResult Delete(DirectoryContext context)
        => SimpleResult.FromAction(context, static state => state.Data.Delete(recursive: true));

    protected override Result<IDirectory> MovetTo(DirectoryContext context, in PathInfo location)
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

    protected override IDirectory SplitDirectoryPath(in PathInfo name)
    {
        string target = Path.Combine(GenericPathHelper.ToRelativePath(OriginalPath), GenericPathHelper.ToRelativePath(name));

        return new LocalDirectory(Context with { Data = new DirectoryInfo(target), NoParent = false }, Features);
    }
}