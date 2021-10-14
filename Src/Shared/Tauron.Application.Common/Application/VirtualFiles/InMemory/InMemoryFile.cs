using System;
using System.IO;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;

namespace Tauron.Application.VirtualFiles.InMemory;

public sealed class InMemoryFile : FileBase<FileContext>
{
    private bool _exist = true;
    
    public InMemoryFile(FileContext context, FileSystemFeature feature) 
        : base(context, feature) { }

    public override FilePath OriginalPath => GenericPathHelper.Combine(Context.Path, Name);

    public override DateTime LastModified => Context.ActualData.ModifyDate;

    public override IDirectory? ParentDirectory => InMemoryDirectory.Create(Context.Parent, Features);

    public override bool Exist => _exist;

    public override string Name => Context.ActualData.ActualName;

    protected override string ExtensionImpl
    {
        get => Path.GetExtension(Context.ActualData.Name);
        set
        {
            Context.ActualData.Name = GenericPathHelper.ChangeExtension(Context.ActualData.Name, value);
            Context.ActualData.ModifyDate = Context.Clock.UtcNow.LocalDateTime;
        }
    }

    public override long Size => Context.ActualData.ActualData.Length;

    protected override Stream CreateStream(FileContext context, FileAccess access, bool createNew)
    {
        if (createNew)
            Context.Root.ReInit(Context.ActualData, Context.Clock);

        return new StreamWrapper(Context.ActualData.ActualData, access, () => Context.ActualData.ModifyDate = Context.Clock.UtcNow.LocalDateTime);
    }

    protected override void Delete(FileContext context)
    {
        if(context.Parent is null) return;

        _exist = !context.Parent.ActualData.Remove(context.ActualData.Name);

        context.Root.ReturnFile(context.ActualData);
        Context = context with { Parent = null, Data = null };
    }

    protected override IFile MoveTo(FileContext context, FilePath location)
    {
        IFile? file = null;

        if (location.Kind == PathType.Absolute)
            file = context.RootSystem.MoveElement(Name, location, context.ActualData,
                (directoryContext, newPath, fileEntry) => 
                    new InMemoryFile(directoryContext.GetFileContext(directoryContext, fileEntry, GenericPathHelper.Combine(newPath, Name)), Features));
        else
        {
            if (ParentDirectory?.GetDirectory(location) is InMemoryDirectory parent)
            {
                if (parent.TryAddElement(Name, context.ActualData))
                {
                    file = parent.GetFile(Name);
                }
            }
        }

        if (file is null)
            throw new InvalidOperationException("Movement Has failed (Possible Duplicate?)");

        context.Parent?.ActualData.Remove(Name);
        Context = context with { Data = null, Parent = null };
        _exist = false;
        
        return file;
    }
}