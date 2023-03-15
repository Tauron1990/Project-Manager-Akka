using System.IO;
using JetBrains.Annotations;
using Stl;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IFile : IFileSystemNode
{
    string Extension { get; set; }

    long Size { get; }

    Result<Stream> Open(FileAccess access);

    Result<Stream> Open();

    Result<Stream> CreateNew();
    
    Result<IFile> MoveTo(in PathInfo location);
}