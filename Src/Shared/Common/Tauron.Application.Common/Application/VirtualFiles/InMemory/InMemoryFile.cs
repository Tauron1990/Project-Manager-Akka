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

    public override PathInfo OriginalPath => Context.Path;

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

    protected override bool CanWrite => Exist && base.CanWrite;

    protected override bool CanCreate => Exist && base.CanCreate;

    protected override Stream CreateStream(FileContext context, FileAccess access, bool createNew)
    {
        if(createNew)
            Context.Root.ReInit(Context.ActualData, Context.Clock);

        return StreamWrapper.Create(Context.ActualData.ActualData, access, _ => Context.ActualData.ModifyDate = Context.Clock.UtcNow.LocalDateTime);
    }

    protected override void Delete(FileContext context)
    {
        if(context.Parent is null) return;

        _exist = !context.Parent.ActualData.Remove(context.ActualData.Name);

        context.Root.ReturnFile(context.ActualData);
        Context = context with { Parent = null, Data = null };
    }


    protected override IFile MoveTo(FileContext context, in PathInfo location)
    {
        string originalName = Name;

        string name;
        PathInfo path;

        if(Path.HasExtension(location.Path))
        {
            name = Path.GetFileName(GenericPathHelper.ToRelativePath(location));
            path = location with
                   {
                       Path = location.Path[..^name.Length],
                   };
        }
        else
        {
            name = Name;
            path = location;
        }


        IFile? file = null;

        if(path.Kind == PathType.Absolute)
        {
            file = context.RootSystem.MoveElement(
                name,
                path,
                context.ActualData,
                (directoryContext, newPath, fileEntry) =>
                    new InMemoryFile(directoryContext.GetFileContext(directoryContext, fileEntry, newPath), Features));
        }
        else
        {
            if(ParentDirectory?.GetDirectory(path) is InMemoryDirectory parent)
                if(parent.TryAddElement(name, context.ActualData))
                    file = parent.GetFile(name);
        }

        if(file is null)
            throw new InvalidOperationException("Movement Has failed (Possible Duplicate?)");

        context.Parent?.ActualData.Remove(originalName);
        Context = context with { Data = null, Parent = null };
        _exist = false;

        return file;
    }
}