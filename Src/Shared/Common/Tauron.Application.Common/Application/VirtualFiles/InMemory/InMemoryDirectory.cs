using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;
using Tauron.Errors;
using Tauron.ObservableExt;

namespace Tauron.Application.VirtualFiles.InMemory;

public class InMemoryDirectory : DirectoryBase<DirectoryContext>
{
    private bool _exist = true;

    private InMemoryDirectory(DirectoryContext context, FileSystemFeature feature) : base(context, feature) { }

    public static IDirectory New(DirectoryContext context, FileSystemFeature feature)
        => new InMemoryDirectory(context, feature);
    
    public override PathInfo OriginalPath => Context.Path;

    public override Result<DateTime> LastModified => Context.ActualData.Select(d => d.ModifyDate);

    public override Result<IDirectory> ParentDirectory =>
        Context.Parent != null
        ? new InMemoryDirectory(Context with { Path = OriginalPath, Data = Context.Parent.Data }, Features)
        : new NoParentDirectory();

    public override bool Exist => _exist;

    public override Result<string> Name => Context.ActualData.Select(d => d.Name);

    internal DirectoryContext DirectoryContext => Context;

    public override Result<IEnumerable<IDirectory>> Directories
        => from data in Context.ActualData
            select data.Directorys.Select(
                d => (IDirectory)new InMemoryDirectory(
                    Context with { Parent = Context, Path = OriginalPath, Data = d },
                    Features));

    public override Result<IEnumerable<IFile>> Files
        => from data in Context.ActualData
            select data.Files.Select(
                f => InMemoryFile.New(
                    Context.GetFileContext(Context, f, OriginalPath),
                    Features));

    public static Result<IDirectory> Create([NotNullIfNotNull("context")] DirectoryContext? context, FileSystemFeature features)
        => context is null ? new NoParentDirectory() : new InMemoryDirectory(context, features);

    internal Result<bool> TryAddElement(string name, IDataElement element)
        => from data in Context.ActualData
            select data.TryAddElement(name, element);

    protected override Result<IDirectory> GetDirectory(DirectoryContext context, PathInfo name)
        => from data in context.Root.GetDirectoryEntry(name, context.Clock)
            select (IDirectory)new InMemoryDirectory(
            context with
            {
                Path = GenericPathHelper.Combine(OriginalPath, name),
                Data = data,
                Parent = Context,
            },
            Features);

    protected override Result<IFile> GetFile(DirectoryContext context, in PathInfo name)
        => InMemoryFile.New(
            Context.GetFileContext(Context, name, OriginalPath),
            Features);

    protected override Result Delete(DirectoryContext context)
    {
        return from name in Name
            from data in context.ActualData
            from parent in context.Parent?.ActualData ?? Result.Ok<DirectoryEntry?>(null!)
            select Apply(data, parent, name);

        Action Apply(DirectoryEntry entry, DirectoryEntry? parent, string name) =>
            () =>
            {
                context.Root.ReturnDirectory(entry);
                parent?.Remove(name);
                Context = context with { Parent = null, Data = null };

                _exist = false;
            };
    }

    private Result<IDirectory> AddTo(in PathInfo path, string name, DirectoryContext context)
        => from dic in GetDirectory(path)
            let newDic = dic as InMemoryDirectory
            where newDic is not null
            from newDicData in newDic.DirectoryContext.ActualData
            select newDicData.TryAddElement(name, newDicData)
                ? Result.Ok(
                    New(context with { Parent = newDic.DirectoryContext, Path = GenericPathHelper.Combine(newDic.OriginalPath, name) }, Features))
                : Result.Fail<IDirectory>(new InvalidOperation("TryAddElement faild"));

    protected override Result<IDirectory> RunMoveTo(DirectoryContext context, PathInfo location)
    {
        return from isok in ValidateSheme(location, "mem")
            from name in Name
            from data in context.ActualData
            let parentDic = ParentDirectory.ValueOrDefault
            from newDic in location.Kind == PathType.Absolute
                ? Context.RootSystem.MoveElement(
                    name,
                    location,
                    data,
                    (newContext, newPath, dic) => New(
                        context with
                        {
                            Parent = newContext,
                            Path = newPath,
                            Data = dic,
                        },
                        Features))
                : parentDic is InMemoryDirectory parent
                    ? parent.AddTo(location, name, context)
                    : Result.Fail<IDirectory>(new InvalidOperation("Directory moving Failed"))
            select Apply(newDic);

        IDirectory Apply(IDirectory newDic)
        {
            _exist = false;
            Context = context with { Parent = null, Data = null };

            return newDic;
        }
    }
}