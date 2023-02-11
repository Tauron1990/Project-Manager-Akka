using System;
using System.Collections.Generic;
using MessagePack.Resolvers;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
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
    public override bool Exist => Context.File is not null;
    public override string Name => GenericPathHelper.GetName(Context.OriginalPath);
    public override IEnumerable<IDirectory> Directories { get; }
    public override IEnumerable<IFile> Files { get; }

    private IEnumerable<IDirectory> FindDirectorys()
    {
        foreach (GridFSFileInfo file in FetchFiles(OriginalPath.Path))
        {
            if(file.Filename.StartsWith(OriginalPath.Path))
            {
                
            }
        }
    }

    private IEnumerable<GridFSFileInfo> FetchFiles(StringOrRegularExpression exp)
    {
        var filter = Builders<GridFSFileInfo>.Filter.StringIn(f => f.Filename, exp);
        var curser = Context.Bucket.Find(filter);

        return curser.ToEnumerable();
    }

    protected override IDirectory GetDirectory(GridFsContext context, in PathInfo name) => throw new NotImplementedException();

    protected override IFile GetFile(GridFsContext context, in PathInfo name) => throw new NotImplementedException();
}