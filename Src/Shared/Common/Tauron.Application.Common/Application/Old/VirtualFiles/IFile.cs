using System.IO;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IFile : IFileSystemNode<IFile>
{
    string Extension { get; }

    Result<IFile> SetExtension(string extension);

    long Size { get; }

    [Pure]
    Result<Stream> Open(FileAccess access);

    [Pure]
    Result<Stream> Open();

    [Pure]
    Result<Stream> CreateNew();
    
    [Pure]
    Result<IFile> MoveTo(in PathInfo location);
}