using System;
using System.IO;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;
using Tauron.Errors;
using Tauron.ObservableExt;
using Result = FluentResults.Result;

namespace Tauron.Application.VirtualFiles.InMemory;

public sealed class InMemoryFile : FileBase<FileContext>
{
    private bool _exist = true;

    private InMemoryFile(FileContext context, FileSystemFeature feature)
        : base(context, feature) { }

    public static IFile New(FileContext context, FileSystemFeature feature)
        => new InMemoryFile(context, feature);

    public static Result<IFile> New(Result<FileContext> contextResult, FileSystemFeature feature)
        => from context in contextResult
            select New(context, feature);

    public override PathInfo OriginalPath => Context.Path;

    public override Result<DateTime> LastModified => Context.ActualData.Bind(md => md.ModifyDate);

    public override Result<IDirectory> ParentDirectory => InMemoryDirectory.Create(Context.Parent, Features);

    public override bool Exist => _exist;

    public override Result<string> Name => Context.ActualData.Bind(d => d.ActualName);

    protected override Result<string> ExtensionImpl =>
        from data in Context.ActualData
        from name in data.ActualName
        select Path.GetExtension(name);

    protected override Result SetExtensionImpl(string extension) =>
        from data in Context.ActualData
        select () =>
        {
            data.Name = GenericPathHelper.ChangeExtension(data.Name, extension);
            data.ModifyDate = Context.Clock.UtcNow.LocalDateTime;
        };

    public override Result<long> Size =>
        from data in Context.ActualData
        from data2 in data.ActualData
        select data2.Length;

    protected override bool CanWrite => Exist && base.CanWrite;

    protected override bool CanCreate => Exist && base.CanCreate;

    protected override Result<Stream> CreateStream(FileContext context, FileAccess access, bool createNew) =>
        from data in Context.ActualData
        from fileentry in createNew
            ? Context.Root.ReInit(data, context.Clock)
            : Result.Ok(data)
        from fileData in fileentry.ActualData
        select StreamWrapper.Create(fileData, access, _ => data.ModifyDate = Context.Clock.UtcNow.LocalDateTime);

    protected override Result Delete(FileContext context)
    {
        if(context.Parent is null) return new NoParentDirectory();

        _exist = false;

        return
            from parentData in Context.Parent!.ActualData
            from data in Context.ActualData
            from name in data.ActualName
            where parentData.Remove(name)
            select () =>
            {
                Context.Root.ReturnFile(data);
                Context = context with { Parent = null, Data = null };
            };
    }

    #pragma warning disable EPS05
    private Result<(string Name, PathInfo Path, string OriginalName)> GetNameData(PathInfo location)
    {
        return from originalName in Name
            let name = Path.GetFileName(GenericPathHelper.ToRelativePath(location))
            select Path.HasExtension(location.Path)
                ? (Path.GetFileName(GenericPathHelper.ToRelativePath(location)), location with { Path = location.Path[..^name.Length] }, originalName)
                : (originalName, location, name);
    }

    private Result<IFile> MoveFileElement(Result<(string Name, PathInfo Path, string OriginalName)> dataResult, FileContext context)
    {
        return from data in dataResult
            from contextData in Context.ActualData
            select MoveElement(data, contextData);

        Func<Result<IFile>> MoveElement((string Name, PathInfo Path, string OriginalName) valueTuple, FileEntry contextData1)
        {

            return valueTuple.Path.Kind == PathType.Absolute
                ? () =>
                    context.RootSystem.MoveElement(
                        valueTuple.Name,
                        valueTuple.Path,
                        contextData1,
                        (directoryContext, newPath, fileEntry) =>
                            (IFile)new InMemoryFile(directoryContext.GetFileContext(directoryContext, fileEntry, newPath), Features))
                : () =>
                    from parent in ParentDirectory
                    from dic in parent.GetDirectory(valueTuple.Path)
                    select dic is InMemoryDirectory newDic && newDic.TryAddElement(valueTuple.Name, contextData1).ValueOrDefault
                        ? newDic.GetFile(valueTuple.Name)
                        : Result.Fail<IFile>(new InvalidOperation().CausedBy("Invalid Path Data or Duplicate"));
        }
    }

    protected override Result<IFile> MoveTo(FileContext context, in PathInfo location)
    {
        var dataResult = GetNameData(location);
        var fileResult = MoveFileElement(dataResult, context);

        return
            from data in dataResult
            from file in fileResult
            select Apply(data.OriginalName, file);


        IFile Apply(string originalName, IFile file)
        {
            context.Parent?.ActualData.ValueOrDefault?.Remove(originalName);
            Context = context with { Data = null, Parent = null };
            _exist = false;

            return file;
        }
    }
}