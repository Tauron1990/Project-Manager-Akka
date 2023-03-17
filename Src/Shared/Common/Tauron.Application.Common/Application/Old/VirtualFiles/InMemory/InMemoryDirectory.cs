using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;
using Tauron.Operations;

namespace Tauron.Application.VirtualFiles.InMemory;

public class InMemoryDirectory : DirectoryBase<DirectoryContext>
{
    private bool _exist = true;

    public InMemoryDirectory(DirectoryContext context, FileSystemFeature feature) : base(context, feature) { }

    public override PathInfo OriginalPath => Context.Path;

    public override DateTime LastModified => Context.ActualData.ModifyDate;

    public override IDirectory? ParentDirectory => Context.Parent != null
        ? new InMemoryDirectory(Context with { Path = OriginalPath, Data = Context.Parent.Data }, Features)
        : null;

    public override bool Exist => _exist;

    public override string Name => Context.ActualData.Name;

    internal DirectoryContext DirectoryContext => Context;

    public override Result<IEnumerable<IDirectory>> Directories()
        => Result.Value(Context.ActualData.Directorys.Select(
            d => (IDirectory)new InMemoryDirectory(
                Context with { Parent = Context, Path = OriginalPath, Data = d },
                Features)));

    public override Result<IEnumerable<IFile>> Files() =>
        Result.Value(Context.ActualData.Files.Select(
            f => (IFile)new InMemoryFile(
                Context.GetFileContext(Context, f, OriginalPath),
                Features)));

    public static InMemoryDirectory? Create([NotNullIfNotNull("context")] DirectoryContext? context, FileSystemFeature features)
        => context is null ? null : new InMemoryDirectory(context, features);

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

    protected override Result<IDirectory> Delete(DirectoryContext context)
    {
        context.Root.ReturnDirectory(context.ActualData);
        context.Parent?.ActualData.Remove(Name);
        Context = context with { Parent = null, Data = null };

        _exist = false;
        
        return SimpleResult.Success();
    }

    private Result<IDirectory> AddTo(in PathInfo path, string name, DirectoryContext context)
        => GetDirectory(path).FlatSelect(
            dic => dic is InMemoryDirectory newDic
                   && newDic.DirectoryContext.ActualData.TryAddElement(name, context.ActualData)
                ? Result.Value<IDirectory>(
                    new InMemoryDirectory(
                        context with
                        {
                            Parent = newDic.DirectoryContext, Path = GenericPathHelper.Combine(newDic.OriginalPath, name),
                        },
                        Features))
                : Result.Error<IDirectory>(new InvalidOperationException($"Dictionary {name} added to {dic.OriginalPath}")));

    protected override Result<IDirectory> MovetTo(DirectoryContext context, in PathInfo location)
    {
        return ValidateSheme(
            location,
            "mem",
            (context, location, self: this),
            static state =>
            {

                Result<IDirectory> newDic = default;

                if(state.location.Kind == PathType.Absolute)
                {
                    newDic = state.context.RootSystem.MoveElement(
                        state.self.Name,
                        state.location,
                        state.context.ActualData,
                        (newContext, newPath, dic) => (IDirectory)new InMemoryDirectory(
                            state.context with
                            {
                                Parent = newContext,
                                Path = newPath,
                                Data = dic,
                            },
                            state.self.Features));
                }
                else
                {
                    if(state.self.ParentDirectory is InMemoryDirectory parent)
                        newDic = parent.AddTo(state.location, state.self.Name, state.context);
                }

                if(newDic.HasValue)
                {
                    state.self._exist = false;
                    state.self.Context = state.context with { Parent = null, Data = null };
                }

                return newDic;
            });
    }

    internal (InMemoryFileSystem FileSystem, DirectoryContext NewParent) UpdateFile(FileContext newFile, FileContext oldFile);
}