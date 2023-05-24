using System.IO;
using JetBrains.Annotations;
using Tauron.Errors;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class FileBase<TContext> : SystemNodeBase<TContext>, IFile
{
    protected FileBase(TContext context, FileSystemFeature feature)
        : base(context, feature, NodeType.File) { }

    protected virtual bool CanRead => Features.HasFlag(FileSystemFeature.Read);
    protected virtual bool CanWrite => Features.HasFlag(FileSystemFeature.Write);
    protected virtual bool CanCreate => Features.HasFlag(FileSystemFeature.Create);

    protected abstract Result<string> ExtensionImpl { get; }

    protected abstract Result SetExtensionImpl(string extension);
    
    public Result<string> Extension => ExtensionImpl;

    public Result SetExtension(string extension) =>
        ValidateFeature(FileSystemFeature.Extension)
            .Bind(() => SetExtensionImpl(extension));

    public abstract Result<long> Size { get; }

    public virtual Result<Stream> Open(FileAccess access) =>
        ValidateFileAcess(access)
            .Bind(() => CreateStream(Context, access, createNew: false));

    public virtual Result<Stream> Open() =>
        ValidateFileAcess(FileAccess.ReadWrite)
            .Bind(() => CreateStream(Context, FileAccess.ReadWrite, createNew: false));

    public virtual Result<Stream> CreateNew()
    {
        return ValidateFileAcess(FileAccess.ReadWrite)
            .Bind(() => Result.OkIf(CanCreate, () => new FeatureNotSupported(FileSystemFeature.Create)))
            .Bind(() => CreateStream(Context, FileAccess.ReadWrite, createNew: true));
    }

    public Result<IFile> MoveTo(PathInfo location) =>
        ValidateFeature(FileSystemFeature.Moveable)
            .Bind(() => MoveTo(Context, GenericPathHelper.NormalizePath(location)));

    private Result ValidateFileAcess(FileAccess access)
    {
        if(access.HasFlag(FileAccess.Read) && !CanRead)
            return new FeatureNotSupported(FileSystemFeature.Read);
        if(access.HasFlag(FileAccess.Write) && !CanWrite)
            return new FeatureNotSupported(FileSystemFeature.Write);

        return Result.Ok();
    }

    protected abstract Result<Stream> CreateStream(TContext context, FileAccess access, bool createNew);

    protected virtual Result<IFile> MoveTo(TContext context, in PathInfo location)
        => Result.Fail(new NotImplemented(nameof(MoveTo)));
}