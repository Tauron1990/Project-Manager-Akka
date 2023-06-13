using System;
using System.Reactive;
using JetBrains.Annotations;
using Tauron.Errors;
using Tauron.ObservableExt;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class SystemNodeBase<TContext> : IFileSystemNode
{
    protected SystemNodeBase(TContext context, FileSystemFeature feature, NodeType nodeType)
    {
        Context = context;
        Features = feature;
        Type = nodeType;
    }

    protected SystemNodeBase(Func<IFileSystemNode, TContext> context, FileSystemFeature feature, NodeType nodeType)
    {
        Context = context(this);
        Features = feature;
        Type = nodeType;
    }

    protected TContext Context { get; set; }

    public FileSystemFeature Features { get; }

    public NodeType Type { get; }

    public abstract PathInfo OriginalPath { get; }

    public abstract Result<DateTime> LastModified { get; }

    public abstract Result<IDirectory> ParentDirectory { get; }

    public abstract bool Exist { get; }

    public abstract Result<string> Name { get; }

    public Result Delete()
    {
        return !Features.HasFlag(FileSystemFeature.Delete) ? new FeatureNotSupported(FileSystemFeature.Delete) : Delete(Context);

    }

    protected virtual Result Delete(TContext context)
        => new NotImplemented(nameof(Delete));

    protected virtual Result ValidateFeature(FileSystemFeature feature) 
        => Result.OkIf(Features.HasFlag(feature), () => new FeatureNotSupported(feature));

    protected Result<Unit> ValidateSheme(in PathInfo info, string scheme)
    {
        return info.Kind == PathType.Relative 
            ? Result.Ok(Unit.Default) 
            : Result.OkIf(GenericPathHelper.HasScheme(info, scheme), new SchemeMismatch(info, scheme)).ToUnit();

    }
}