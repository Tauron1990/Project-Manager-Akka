using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IFile : IFileSystemNode
{
    string Extension { get; set; }

    long Size { get; }

    Stream Open(FileAccess access);

    Stream Open();

    Stream CreateNew();

    IFile MoveTo(PathInfo location);
}