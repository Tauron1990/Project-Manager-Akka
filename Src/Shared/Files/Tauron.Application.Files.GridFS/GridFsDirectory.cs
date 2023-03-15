using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Stl;
using Tauron.Application.Files.GridFS.Core;
using Tauron.Application.VirtualFiles;
using Tauron.Application.VirtualFiles.Core;

namespace Tauron.Application.Files.GridFS;

public sealed class GridFsDirectory : DirectoryBase<GridFsContext>
{
    public GridFsDirectory(GridFsContext context, NodeType nodeType) 
        : base(context, FileSystemFeature.None, nodeType) { }

    public override PathInfo OriginalPath => Context.OriginalPath;
    public override DateTime LastModified => Context.File?.UploadDateTime ?? DateTime.MinValue;
    public override IDirectory? ParentDirectory => Context.Parent;
    public override bool Exist => true;
    public override string Name => GenericPathHelper.GetName(Context.OriginalPath);
    public override IEnumerable<IDirectory> Directories => FindDirectorys();
    public override Result<IEnumerable<IFile>> GetFiles() => FindFiles();

    private IEnumerable<IDirectory> FindDirectorys()
    {
        #pragma warning disable EPS06
        foreach (GridFSFileInfo file in FetchFiles(OriginalPath.Path))
        {
            if(file.Filename.StartsWith(OriginalPath.Path, StringComparison.Ordinal))
            {
                var span = file.Filename.AsSpan();
                span = span[OriginalPath.Path.Length..];
                int index = span.IndexOf(GenericPathHelper.GenericSeperator);
                if(index == -1)
                    continue;

                yield return new GridFsDirectory(
                    Context with
                    {
                        OriginalPath = GenericPathHelper.Combine(OriginalPath, span[..index].ToString()),
                        Parent = this,
                    },
                    NodeType.Directory);
            }
        }
    }
    
    private IEnumerable<IFile> FindFiles()
    {
        foreach (GridFSFileInfo file in FetchFiles(OriginalPath.Path))
        {
            if(file.Filename.StartsWith(OriginalPath.Path, StringComparison.Ordinal))
            {
                var span = file.Filename.AsSpan();
                span = span[OriginalPath.Path.Length..];
                int index = span.IndexOf(GenericPathHelper.GenericSeperator);
                if(index != -1)
                    continue;

                yield return new GridFsFile(
                    Context with
                    {
                        File = file,
                        OriginalPath = GenericPathHelper.Combine(OriginalPath, span[..index].ToString()),
                        Parent = this,
                    });
            }
        }
    }

    private IEnumerable<GridFSFileInfo> FetchFiles(StringOrRegularExpression exp)
    {
        var filter = Builders<GridFSFileInfo>.Filter.StringIn(f => f.Filename, exp);
        var curser = Context.Bucket.Find(filter);

        return curser.ToEnumerable();
    }

    protected override Result<IDirectory> GetDirectory(GridFsContext context, in PathInfo name) 
        => new GridFsDirectory(
            context with
            {
                OriginalPath = GenericPathHelper.Combine(OriginalPath, name),
                Parent = this,
            },
            NodeType.Directory);

    protected override Result<IFile> GetFile(GridFsContext context, in PathInfo name)
    {
        PathInfo newPath = GenericPathHelper.Combine(OriginalPath, name);
        GridFSFileInfo? entry = context.FindEntry(newPath.Path);

        return new GridFsFile(context with { File = entry, OriginalPath = newPath });
    }
}