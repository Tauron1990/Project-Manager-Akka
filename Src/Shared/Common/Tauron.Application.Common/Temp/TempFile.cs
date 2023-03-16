using System.IO;
using Tauron.Application.VirtualFiles;
using Tauron.Application.VirtualFiles.LocalVirtualFileSystem;

namespace Tauron.Temp;

public sealed class TempFile : DisposeableBase, ITempFile
{
    private readonly IFile? _file;
    private Stream? _targetStream;

    public TempFile(IFile? file, Option<ITempDic> parent)
    {
        _file = file;
        Parent = parent;
    }

    internal Stream InternalStrem => _targetStream ??= CreateStream();

    public Option<ITempDic> Parent { get; }

    public bool NoStreamDispose { get; set; }

    public Stream Stream => new TempStream(this, false);

    public Stream NoDisposeStream => new TempStream(this, true);

    public PathInfo FullPath => _file?.OriginalPath ?? string.Empty;

    private Stream CreateStream()
    {
        if(_file is IFullFileStreamSupport fileStreamSupport)
            return fileStreamSupport.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Delete | FileShare.Read, 4096, FileOptions.DeleteOnClose);

        return _file?.Open(FileAccess.ReadWrite) ?? Stream.Null;
    }

    protected override void DisposeCore(bool disposing) => _targetStream?.Dispose();
}