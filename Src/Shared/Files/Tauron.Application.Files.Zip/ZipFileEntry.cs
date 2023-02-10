using System;
using System.IO;
using Ionic.Zip;
using Tauron.Application.Files.Zip.Data;
using Tauron.Application.VirtualFiles;
using Tauron.Application.VirtualFiles.Core;
using Tauron.Application.VirtualFiles.InMemory.Data;

namespace Tauron.Application.Files.Zip;

public sealed class ZipFileEntry : FileBase<ZipContext>
{
    public ZipFileEntry(ZipContext context, FileSystemFeature feature) 
        : base(context, feature) { }

    public override PathInfo OriginalPath => Context.ConstructOriginalPath();
    public override DateTime LastModified => Context.Entry?.ModifiedTime ?? DateTime.MinValue;
    public override IDirectory? ParentDirectory => Context.CreateParent(this);
    public override bool Exist => Context.Entry != null;
    public override string Name => Context.Name;
    protected override string ExtensionImpl
    {
        get => Path.GetExtension(ValidateFileExist(Context).FileName);
        set
        {
            var entry = ValidateFileExist(Context);
            entry.FileName = Path.ChangeExtension(entry.FileName, value);
        }
    }

    public override long Size
    {
        get
        {
            var entry = ValidateFileExist(Context);

            if (entry.UncompressedSize != 0)
                return entry.UncompressedSize;

            if (entry.CompressedSize != 0)
                return entry.CompressedSize;

            if (entry.Source == ZipEntrySource.Stream)
                return entry.InputStream?.Length ?? 0;

            return 0;
        }
    }

    protected override Stream CreateStream(ZipContext context, FileAccess access, bool createNew)
    {
        ZipEntry? entry = ValidateFileExist(context);


        if(!access.HasFlag(FileAccess.Write))
            return entry.OpenReader();

        return new StreamWrapper(InMemoryRoot.Manager.GetStream(), FileAccess.ReadWrite, ModifyAction);


        void ModifyAction(MemoryStream mem)
        {
            context.File.RemoveEntry(entry);
            context.File.AddEntry()
        }
    }

    private ZipEntry ValidateFileExist(ZipContext context)
    {
        if (context.Entry is null) 
            Context = context with { Entry = context.File.AddEntry(OriginalPath, Array.Empty<byte>()) };

        return Context.Entry!;
    }
}