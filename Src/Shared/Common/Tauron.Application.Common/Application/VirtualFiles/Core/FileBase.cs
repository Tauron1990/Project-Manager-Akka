using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class FileBase<TContext> : SystemNodeBase<TContext>, IFile
{
    protected FileBase(TContext context, FileSystemFeature feature)
        : base(context, feature, NodeType.File) { }

    protected virtual bool CanRead => Features.HasFlag(FileSystemFeature.Read);
    protected virtual bool CanWrite => Features.HasFlag(FileSystemFeature.Write);
    protected virtual bool CanCreate => Features.HasFlag(FileSystemFeature.Create);

    protected abstract string ExtensionImpl { get; set; }

    public string Extension
    {
        get => ExtensionImpl;
        set
        {
            ValidateFeature(FileSystemFeature.Extension);
            ExtensionImpl = value;
        }
    }

    public abstract long Size { get; }

    public virtual Stream Open(FileAccess access)
    {
        ValidateFileAcess(access);

        return CreateStream(Context, access, false);
    }

    public virtual Stream Open()
    {
        ValidateFileAcess(FileAccess.ReadWrite);

        return CreateStream(Context, FileAccess.ReadWrite, false);
    }

    public virtual Stream CreateNew()
    {
        ValidateFileAcess(FileAccess.ReadWrite);

        if(!CanCreate)
            throw new IOException("File can not Created");

        return CreateStream(Context, FileAccess.ReadWrite, true);
    }

    public IFile MoveTo(PathInfo location)
    {
        ValidateFeature(FileSystemFeature.Moveable);

        return MoveTo(Context, GenericPathHelper.NormalizePath(location));
    }

    private void ValidateFileAcess(FileAccess access)
    {
        if(access.HasFlag(FileAccess.Read) && !CanRead)
            throw new IOException("File canot be Read");
        if(access.HasFlag(FileAccess.Write) && !CanWrite)
            throw new IOException("File can not be Write");
    }

    protected abstract Stream CreateStream(TContext context, FileAccess access, bool createNew);

    protected virtual IFile MoveTo(TContext context, PathInfo location)
        => throw new IOException("Move is not Implemented");
}