using System;
using System.IO;
using Tauron.Operations;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class SystemNodeBase<TSelf, TContext> : IFileSystemNode<TSelf> 
    where TSelf : IFileSystemNode<TSelf>
{
    protected SystemNodeBase(TContext context, FileSystemFeature feature, NodeType nodeType)
    {
        Context = context;
        Features = feature;
        Type = nodeType;
    }

    protected SystemNodeBase(Func<IFileSystemNode<TSelf>, TContext> context, FileSystemFeature feature, NodeType nodeType)
    {
        Context = context(this);
        Features = feature;
        Type = nodeType;
    }
    
    protected TContext Context { get; }

    public FileSystemFeature Features { get; }

    public NodeType Type { get; }

    public abstract PathInfo OriginalPath { get; }

    public abstract DateTime LastModified { get; }

    public abstract IDirectory? ParentDirectory { get; }

    public abstract bool Exist { get; }

    public abstract string Name { get; }

    [Pure]
    public Result<TSelf> Delete() => 
        !Features.HasFlag(FileSystemFeature.Delete) 
            ? Result.Error<TSelf>(new IOException("Delete not Supported")) 
            : Delete(Context);

    [Pure]
    protected virtual Result<TSelf> Delete(TContext context)
        => Result.Error<TSelf>(new InvalidOperationException("Delete not Implemented"));

    [Pure]
    protected virtual Result<TResult> ValidateFeature<TResult, TState>(
        FileSystemFeature feature, 
        TState state,  
        Func<TState, Result<TResult>> isOk) => 
        Features.HasFlag(feature) 
            ? isOk(state)
            : Result.Error<TResult>(new IOException($"Requested Flag {feature} is not set for {GetType().Name}"));
    
    [Pure]
    protected virtual SimpleResult ValidateFeature<TState>(
        FileSystemFeature feature, 
        TState state,  
        Func<TState, SimpleResult> isOk) => 
        Features.HasFlag(feature) 
            ? isOk(state)
            : SimpleResult.Failure($"Requested Flag {feature} is not set for {GetType().Name}");

    [Pure]
    protected SimpleResult ValidateSheme(in PathInfo info, string scheme, Func<SimpleResult> isOk)
    {
        return info.Kind == PathType.Relative || GenericPathHelper.HasScheme(info, scheme)
                ? isOk()
                : SimpleResult.Failure($"Invalid Absolute Path Scheme ({scheme})");

    }
    
    [Pure]
    protected Result<TResult> ValidateSheme<TResult, TState>(in PathInfo info, string scheme, TState state, Func<TState, Result<TResult>> isOk)
    {
        return info.Kind == PathType.Relative || GenericPathHelper.HasScheme(info, scheme)
            ? isOk(state)
            : Result.Error<TResult>(new InvalidOperationException($"Invalid Absolute Path Scheme ({scheme})"));

    }
}