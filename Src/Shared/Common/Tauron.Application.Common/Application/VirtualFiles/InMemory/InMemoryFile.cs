using System;
using System.IO;
using Stl;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;
using Tauron.Operations;

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

    protected override Result<Stream> CreateStream(FileContext context, FileAccess access, bool createNew) =>
        Result.FromFunc(
            (self: this, context, access, createNew),
            static state =>
            {
                if(state.createNew)
                    state.context.Root.ReInit(state.context.ActualData, state.context.Clock);

                return StreamWrapper.Create(
                    state.context.ActualData.ActualData,
                    state.access,
                    _ => state.context.ActualData.ModifyDate = state.context.Clock.UtcNow.LocalDateTime);
            });

    protected override SimpleResult Delete(FileContext context)
    {
        if(context.Parent is null) return SimpleResult.Failure("File Has no Parent");

        return SimpleResult.FromAction(
            (self:this, context),
            static state =>
            {
                state.self._exist = !state.context.Parent!.ActualData.Remove(state.context.ActualData.Name);

                state.context.Root.ReturnFile(state.context.ActualData);
                state.self.Context = state.context with { Parent = null, Data = null };
            });
    }


    protected override Result<IFile> MoveTo(FileContext context, in PathInfo location)
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


        Result<IFile> file;

        if(path.Kind == PathType.Absolute)
        {
            file = context.RootSystem.MoveElement(
                name,
                path,
                context.ActualData,
                (directoryContext, newPath, fileEntry) =>
                    (IFile)new InMemoryFile(directoryContext.GetFileContext(directoryContext, fileEntry, newPath), Features));
        }
        else
        {
            if(ParentDirectory is null)
                file = Result.Error<IFile>(new InvalidOperationException("No Parent Directory"));
            else
            {
                file = ParentDirectory.GetDirectory(path)
                    .FlatSelect(
                        dic =>
                        {
                            var memdic = (InMemoryDirectory)dic;
                            
                            return memdic.TryAddElement(name, context.ActualData) 
                                ? memdic.GetFile(name) 
                                : Result.Error<IFile>(new InvalidOperationException("Adding new File Data Failed"));

                        });
            }
        }

        if(file.HasValue)
        {
            context.Parent?.ActualData.Remove(originalName);
            Context = context with { Data = null, Parent = null };
            _exist = false;
        }

        return file;
    }
}