using System;
using System.Collections.Generic;
using System.Linq;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;

namespace Tauron.Application.VirtualFiles.InMemory;

public class InMemoryDirectory : DirectoryBase<DirectoryContext>
{
    public InMemoryDirectory(DirectoryContext context, FileSystemFeature feature) : base(context, feature) { }

    public override string OriginalPath => GenericPathHelper.Combine(Context.Path, Name);

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
                new FileContext(Context.Root, this, f, OriginalPath, Context.Clock),
                Features));
    
    protected override IDirectory GetDirectory(DirectoryContext context, string name)
        => new InMemoryDirectory(context with
                                 {
                                     Path = OriginalPath, 
                                     Data = Context.Root.GetDirectoryEntry(name, context.Clock),
                                     Parent = this
                                 }, Features);

    protected override IFile GetFile(DirectoryContext context, string name)
        => new InMemoryFile(
            new FileContext(context.Root, this, context.Root.GetInitializedFile(name, context.Clock), OriginalPath, context.Clock),
            Features);
}