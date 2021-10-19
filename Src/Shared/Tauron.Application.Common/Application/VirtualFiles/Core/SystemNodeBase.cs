﻿using System;
using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles.Core;

[PublicAPI]
public abstract class SystemNodeBase<TContext> : IFileSystemNode
{
    protected TContext Context { get; set; }

    public FileSystemFeature Features { get; }
        
    public NodeType Type { get; }
        
    public abstract PathInfo OriginalPath { get; }
        
    public abstract DateTime LastModified { get; }
        
    public abstract IDirectory? ParentDirectory { get; }

    public abstract bool Exist { get; } 
        
    public abstract string Name { get; }

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

    public void Delete()
    {
        if(!Features.HasFlag(FileSystemFeature.Delete)) return;
            
        Delete(Context);
    }

    protected virtual void Delete(TContext context)
        => throw new IOException("Delete not Implemented");

    protected virtual void ValidateFeature(FileSystemFeature feature)
    {
        if(Features.HasFlag(feature)) return;

        throw new IOException($"Requested Flag {feature} is not set for {GetType().Name}");
    }

    protected void ValidateSheme(PathInfo info, string scheme)
    {
        if(info.Kind == PathType.Relative) return;

        if (!GenericPathHelper.HasScheme(info, scheme))
            throw new IOException($"Invalid Absolute Path Scheme ({scheme})");
    }
}