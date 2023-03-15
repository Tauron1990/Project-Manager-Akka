using System;
using System.IO;
using JetBrains.Annotations;
using Stl;
using Tauron.Operations;

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
            ValidateFeature(
                FileSystemFeature.Extension,
                (self:this, value),
                state => Result.Value(state.self.ExtensionImpl = state.value))
#pragma warning disable EPS06
                .ThrowIfError();
#pragma warning restore EPS06
        }
    }

    public abstract long Size { get; }

    public virtual Result<Stream> Open(FileAccess access) =>
        ValidateFileAcess(
            access,
            (self: this, access),
            state => state.self.CreateStream(state.self.Context, state.access, createNew: false));

    public virtual Result<Stream> Open() =>
        ValidateFileAcess(
            FileAccess.ReadWrite,
            this,
            s => s.CreateStream(s.Context, FileAccess.ReadWrite, createNew: false));

    public virtual Result<Stream> CreateNew() =>
        ValidateFileAcess(
            FileAccess.ReadWrite,
            this,
            s => s.CanCreate
                ? CreateStream(s.Context, FileAccess.ReadWrite, createNew: true)
                : Result.Error<Stream>(new IOException("File can not Created")));

    public Result<IFile> MoveTo(in PathInfo location)
    {
        return ValidateFeature(
            FileSystemFeature.Moveable,
            (self:this, location), 
            state => state.self.MoveTo(state.self.Context, state.location));
    }

    private Result<TData> ValidateFileAcess<TData, TState>(FileAccess access, TState state, Func<TState, Result<TData>> isOk)
    {
        if(access.HasFlag(FileAccess.Read) && !CanRead)
            return Result.Error<TData>(new IOException("File canot be Read"));
        if(access.HasFlag(FileAccess.Write) && !CanWrite)
            return Result.Error<TData>(new IOException("File can not be Write"));

        return isOk(state);
    }

    protected abstract Result<Stream> CreateStream(TContext context, FileAccess access, bool createNew);

    protected virtual Result<IFile> MoveTo(TContext context, in PathInfo location)
        => throw new IOException("Move is not Implemented");
}