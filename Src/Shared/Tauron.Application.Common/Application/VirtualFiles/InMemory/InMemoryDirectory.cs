﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;

namespace Tauron.Application.VirtualFiles.InMemory;

public class InMemoryDirectory : DirectoryBase<DirectoryContext>
{
    public InMemoryDirectory(DirectoryContext context, FileSystemFeature feature) : base(context, feature) { }
    
    public static InMemoryDirectory? Create([NotNullIfNotNull("context")]DirectoryContext? context, FileSystemFeature features)
        => context is null ? null : new InMemoryDirectory(context, features);

    public override FilePath OriginalPath => GenericPathHelper.Combine(Context.Path, Name);

    public override DateTime LastModified => Context.Data.ModifyDate;

    public override IDirectory? ParentDirectory => Context.Parent;
    public override bool Exist => true;

    public override string Name => Context.Data.Name;

    public override IEnumerable<IDirectory> Directories
        => Context.Data.Directorys.Select(
            d => new InMemoryDirectory(
                Context with { Parent = this, Path = OriginalPath, Data = d },
                Features));

    public override IEnumerable<IFile> Files
        => Context.Data.Files.Select(
            f => new InMemoryFile(
                Context.GetFileContext(this, f, OriginalPath),
                Features));
    
    protected override IDirectory GetDirectory(DirectoryContext context, FilePath name)
        => new InMemoryDirectory(context with
                                 {
                                     Path = OriginalPath, 
                                     Data = Context.Root.GetDirectoryEntry(name, context.Clock),
                                     Parent = this
                                 }, Features);

    protected override IFile GetFile(DirectoryContext context, FilePath name)
        => new InMemoryFile(
            Context.GetFileContext(this, name, OriginalPath),
            Features);
}