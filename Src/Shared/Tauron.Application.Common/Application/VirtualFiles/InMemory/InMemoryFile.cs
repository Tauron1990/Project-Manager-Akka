using System;
using System.IO;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;

namespace Tauron.Application.VirtualFiles.InMemory;

public sealed class InMemoryFile : FileBase<FileContext>
{
    public InMemoryFile(FileContext context, FileSystemFeature feature) 
        : base(context, feature) { }

    public override string OriginalPath => Context.Path;

    public override DateTime LastModified => Context.Data.ModifyDate;

    public override IDirectory? ParentDirectory => Context.Parent;

    public override string Name => Context.Data.ActualName;

    protected override string ExtensionImpl
    {
        get => Path.GetExtension(Context.Data.Name);
        set
        {
            Context.Data.Name = GenericPathHelper.ChangeExtension(Context.Data.Name, value);
            Context.Data.ModifyDate = Context.Clock.UtcNow.LocalDateTime;
        }
    }

    public override long Size => Context.Data.ActualData.Length;

    protected override Stream CreateStream(FileContext context, FileAccess access, bool createNew)
    {
        if (createNew)
            Context.Root.ReInit(Context.Data, Context.Clock);

        return new StreamWrapper(Context.Data.ActualData, access, () => Context.Data.ModifyDate = Context.Clock.UtcNow.LocalDateTime);
    }
}