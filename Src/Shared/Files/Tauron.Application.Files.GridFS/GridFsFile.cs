using System;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using Tauron.Application.Files.GridFS.Core;
using Tauron.Application.VirtualFiles;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;

namespace Tauron.Application.Files.GridFS;

public sealed class GridFsFile : FileBase<GridFsContext>
{
    private const FileSystemFeature FileFutures = FileSystemFeature.Create | FileSystemFeature.Delete | FileSystemFeature.Extension |
                                                  FileSystemFeature.Read | FileSystemFeature.Write;


    public GridFsFile(GridFsContext context) : base(context, FileFutures)
    {
    }

    public override PathInfo OriginalPath => Context.OriginalPath;
    public override DateTime LastModified => Context.File?.UploadDateTime ?? DateTime.MinValue;
    public override IDirectory? ParentDirectory => Context.Parent;
    public override bool Exist => Context.File is not null;
    public override string Name => GenericPathHelper.GetName(Context.OriginalPath);
    protected override string ExtensionImpl
    {
        get => Path.GetExtension(Context.OriginalPath);
        set
        {
            PathInfo newPath = GenericPathHelper.ChangeExtension(Context.OriginalPath, value);
            
            if(Context.File is null)
            {
                Context = Context with { OriginalPath = newPath };
                return;
            }

            Context.Bucket.Rename(Context.File.Id, newPath);
            Context = Context with
            {
                OriginalPath = newPath,
                File = Context.FindEntry(Context.File.Id),
            };
        }
    }

    public override long Size => Context.File?.Length ?? 0;

    protected override Stream CreateStream(GridFsContext context, FileAccess access, bool createNew)
    {
        switch (access)
        {

            case FileAccess.Read:
                if(context.File is null)
                    throw new InvalidOperationException("File does not Exist");

                return context.Bucket.OpenDownloadStream(context.File.Id);
            case FileAccess.Write:

                var upload = context.File is null 
                    ? context.Bucket.OpenUploadStream(ObjectId.GenerateNewId(),  OriginalPath.Path) 
                    : context.Bucket.OpenUploadStream(context.File.Id, context.File.Filename);

                return StreamWrapper.Create(upload, access, UpdateFileInfo);
            case FileAccess.ReadWrite:
            default:
                throw new InvalidOperationException("File Could either Read or Write not Both");
        }
    }

    private void UpdateFileInfo(GridFSUploadStream<ObjectId> s)
    {
        s.Dispose();

        Context = Context with { File = Context.FindEntry(s.Id) };
    }

    protected override void Delete(GridFsContext context)
    {
        if(context.File is null) return;
        
        context.Bucket.Delete(context.File.Id);

        Context = context with { File = null };
    }
}