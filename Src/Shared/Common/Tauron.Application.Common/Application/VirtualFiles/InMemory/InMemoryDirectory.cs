using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;
using Tauron.ObservableExt;

namespace Tauron.Application.VirtualFiles.InMemory;

public class InMemoryDirectory : DirectoryBase<DirectoryContext>
{
    private bool _exist = true;

    public InMemoryDirectory(DirectoryContext context, FileSystemFeature feature) : base(context, feature) { }

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
            select d
    //          => Result.Ok(
        //    Context.ActualData.Directorys.Select(
          //      d => (IDirectory)new InMemoryDirectory(
            //        Context with { Parent = Context, Path = OriginalPath, Data = d },
              //      Features)));

    public override Result<IEnumerable<IFile>> Files
        => Result.Ok(
            Context.ActualData.Files.Select(
            f => (IFile)new InMemoryFile(
                Context.GetFileContext(Context, f, OriginalPath),
                Features)));

    public static Result<IDirectory> Create([NotNullIfNotNull("context")] DirectoryContext? context, FileSystemFeature features)
        => context is null ? new NoParentDirectory() : new InMemoryDirectory(context, features);

    internal bool TryAddElement(string name, IDataElement element)
        => Context.ActualData.TryAddElement(name, element);

    protected override Result<IDirectory> GetDirectory(DirectoryContext context, in PathInfo name)
        => new InMemoryDirectory(
            context with
            {
                Path = GenericPathHelper.Combine(OriginalPath, name),
                Data = Context.Root.GetDirectoryEntry(name, context.Clock),
                Parent = Context,
            },
            Features);

    protected override Result<IFile> GetFile(DirectoryContext context, in PathInfo name)
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

    private IDirectory? AddTo(in PathInfo path, string name, DirectoryContext context)
        => GetDirectory(path) is InMemoryDirectory newDic
        && newDic.DirectoryContext.ActualData.TryAddElement(name, context.ActualData)
            ? new InMemoryDirectory(context with { Parent = newDic.DirectoryContext, Path = GenericPathHelper.Combine(newDic.OriginalPath, name) }, Features)
            : null;

    protected override FluentResults.Result<IDirectory> RunMoveTo(DirectoryContext context, in PathInfo location)
    {
        ValidateSheme(location, "mem");

        IDirectory? newDic = null;

        if(location.Kind == PathType.Absolute)
        {
            newDic = Context.RootSystem.MoveElement(
                Name,
                location,
                context.ActualData,
                (newContext, newPath, dic) => new InMemoryDirectory(
                    context with
                    {
                        Parent = newContext,
                        Path = newPath,
                        Data = dic,
                    },
                    Features));
        }
        else
        {
            if(ParentDirectory is InMemoryDirectory parent)
                newDic = parent.AddTo(location, Name, context);
        }

        if(newDic is null)
            throw new InvalidOperationException("Directory moving Failed");

        _exist = false;
        Context = context with { Parent = null, Data = null };

        return newDic;
    }
}