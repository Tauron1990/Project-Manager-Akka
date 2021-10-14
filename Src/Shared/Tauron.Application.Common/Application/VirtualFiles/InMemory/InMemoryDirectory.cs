﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;

namespace Tauron.Application.VirtualFiles.InMemory;

public class InMemoryDirectory : DirectoryBase<DirectoryContext>
{
    private bool _exist = true;
    
    public InMemoryDirectory(DirectoryContext context, FileSystemFeature feature) : base(context, feature) { }
    
    public static InMemoryDirectory? Create([NotNullIfNotNull("context")]DirectoryContext? context, FileSystemFeature features)
        => context is null ? null : new InMemoryDirectory(context, features);

    public override FilePath OriginalPath => GenericPathHelper.Combine(Context.Path, Name);

    public override DateTime LastModified => Context.ActualData.ModifyDate;

    public override IDirectory? ParentDirectory => Context.Parent != null
        ? new InMemoryDirectory(Context with { Path = OriginalPath, Data = Context.Parent.Data }, Features)
        : null;
    
    public override bool Exist => _exist;

    public override string Name => Context.ActualData.Name;

    internal DirectoryContext DirectoryContext => Context;
    
    public override IEnumerable<IDirectory> Directories
        => Context.ActualData.Directorys.Select(
            d => new InMemoryDirectory(
                Context with { Parent = this.Context, Path = OriginalPath, Data = d },
                Features));

    public override IEnumerable<IFile> Files
        => Context.ActualData.Files.Select(
            f => new InMemoryFile(
                Context.GetFileContext(Context, f, OriginalPath),
                Features));

    internal bool TryAddElement(string name, IDataElement element)
        => Context.ActualData.TryAddElement(name, element);
    
    protected override IDirectory GetDirectory(DirectoryContext context, FilePath name)
        => new InMemoryDirectory(context with
                                 {
                                     Path = OriginalPath, 
                                     Data = Context.Root.GetDirectoryEntry(name, context.Clock),
                                     Parent = Context
                                 }, Features);

    protected override IFile GetFile(DirectoryContext context, FilePath name)
        => new InMemoryFile(
            Context.GetFileContext(Context, name, OriginalPath),
            Features);

    protected override void Delete(DirectoryContext context)
    {
        context.Root.ReturnDirectory(context.ActualData);
        context.Parent?.ActualData.Remove(Name);
        Context = context with { Parent = null, Data = null };

        _exist = false;
    }

    private IDirectory? AddTo(FilePath path, string name, DirectoryContext context)
        => GetDirectory(path) is InMemoryDirectory newDic && newDic.DirectoryContext.ActualData.TryAddElement(name, context.ActualData) 
            ? new InMemoryDirectory(context with { Parent = newDic.DirectoryContext }, Features) : null;

    protected override IDirectory MovetTo(DirectoryContext context, FilePath location)
    {
        IDirectory? newDic = null;
        
        if (location.Kind == PathType.Absolute)
        {
            newDic = Context.RootSystem.MoveElement(
                Name,
                location,
                context.ActualData,
                (newContext, newPath, dic) => new InMemoryDirectory(
                    context with
                    {
                        Parent = newContext,
                        Path = GenericPathHelper.Combine(newPath, Name),
                        Data = dic
                    },
                    Features));
        }
        else
        {
            if (ParentDirectory is InMemoryDirectory parent) 
                newDic = parent.AddTo(location, Name, context);
        }

        if (newDic is null)
            throw new InvalidOperationException("Directory moving Failed");

        Context = context with { Parent = null, Data = null };

        return newDic;
    }
}