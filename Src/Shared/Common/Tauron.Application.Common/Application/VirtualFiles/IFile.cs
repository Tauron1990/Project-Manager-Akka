using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IFile : IFileSystemNode
{
    Result<string> Extension { get; }

    Result<long> Size { get; }

    Result SetExtension(string extension);
    
    Result<Stream> Open(FileAccess access);

    Result<Stream> Open();

    Result<Stream> CreateNew();

    Result<IFile> MoveTo(PathInfo location);
}