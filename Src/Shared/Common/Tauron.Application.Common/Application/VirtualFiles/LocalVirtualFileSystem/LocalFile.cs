using System;
using System.IO;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.Resolvers;

namespace Tauron.Application.VirtualFiles.LocalVirtualFileSystem;

public sealed class LocalFile : FileBase<FileContext>, IHasFileAttributes, IFullFileStreamSupport
{
    public LocalFile(FileContext context, FileSystemFeature feature)
        : base(context, feature) { }

    public override PathInfo OriginalPath => Context.Data.FullName;
    public override DateTime LastModified => Context.Data.LastWriteTime;

    public override IDirectory? ParentDirectory
    {
        get
        {
            if(Context.NoParent) return null;

            DirectoryInfo? parent = Context.Data.Directory;

            return parent is null ? null : new LocalDirectory(new DirectoryContext(Context.Root, Context.NoParent, parent), Features);
        }
    }

    public override bool Exist => Context.Data.Exists;
    public override string Name => Context.Data.Name;

    protected override string ExtensionImpl
    {
        get => Context.Data.Extension;
        set
        {
            string source = Context.Data.FullName;
            string target = Path.ChangeExtension(source, value);
            File.Move(source, target);
            Context = Context with { Data = new FileInfo(target) };
        }
    }

    public override long Size => Context.Data.Length;

    public Stream Open(FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
        => new FileStream(Context.Data.FullName, mode, access, share, bufferSize, options);

    public FileAttributes Attributes
    {
        get => Context.Data.Attributes;
        set => Context.Data.Attributes = value;
    }

    protected override Stream CreateStream(FileContext context, FileAccess access, bool createNew)
    {
        // ReSharper disable once InvertIf
        if(createNew)
        {
            IDirectory? dic = ParentDirectory;
            if(dic is { Exist: false }) Directory.CreateDirectory(dic.OriginalPath);
        }

        return context.Data.Open(createNew ? FileMode.Create : FileMode.Open, access);
    }

    protected override void Delete(FileContext context)
        => context.Data.Delete();

    protected override IFile MoveTo(FileContext context, in PathInfo location)
    {
        ValidateSheme(location, LocalFileSystemResolver.SchemeName);

        string target = location.Kind == PathType.Absolute
            ? Path.GetFullPath(location.Path)
            : Path.GetFullPath(context.Root, location.Path);
        File.Move(OriginalPath, target);

        return new LocalFile(context with { Data = new FileInfo(target) }, Features);
    }
}