using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class FileBase<TContext> : SystemNodeBase<TContext>, IFile
{
    protected FileBase(TContext context, FileSystemFeature feature) 
        : base(context, feature, NodeType.File) { }

    protected bool CanRead => Features.HasFlag(FileSystemFeature.Read);
    protected bool CanWrite => Features.HasFlag(FileSystemFeature.Write);
    protected bool CanCreate => Features.HasFlag(FileSystemFeature.Create);

    public string Extension
    {
        get => ExtensionImpl;
        set
        {
            ValidateFeature(FileSystemFeature.Extension);
            ExtensionImpl = value;
        }
    }
        
    protected abstract string ExtensionImpl { get; set; }
        
    public abstract long Size { get; }

    private void ValidateFileAcess(FileAccess access)
    {
        if (access.HasFlag(FileAccess.Read) && !CanRead)
            throw new IOException("File canot be Read");
        if (access.HasFlag(FileAccess.Write) && !CanWrite)
            throw new IOException("File can not be Write");
    }
        
    public virtual Stream Open(FileAccess access)
    {
        ValidateFileAcess(access);
            
        return CreateStream(Context, access, createNew: false);
    }

    public virtual Stream Open()
    {
        ValidateFileAcess(FileAccess.ReadWrite);
            
        return CreateStream(Context, FileAccess.ReadWrite, createNew: false);
    }

    public virtual Stream CreateNew()
    {
        ValidateFileAcess(FileAccess.ReadWrite);

        if (!CanCreate)
            throw new IOException("File can not Created");
            
        return CreateStream(Context, FileAccess.ReadWrite, createNew: true);
    }

    protected abstract Stream CreateStream(TContext context, FileAccess access, bool createNew);

    public IFile MoveTo(string location)
    {
        ValidateFeature(FileSystemFeature.Moveable);

        return MoveTo(Context, GenericPathHelper.NormalizePath(location));
    }

    protected virtual IFile MoveTo(TContext context, string location)
        => throw new IOException("Move is not Implemented");
}