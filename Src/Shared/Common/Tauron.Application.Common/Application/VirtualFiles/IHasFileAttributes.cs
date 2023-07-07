using System.IO;

namespace Tauron.Application.VirtualFiles;

public interface IHasFileAttributes
{
    Result<FileAttributes> Attributes { get; }

    Result SetFileAttributes(FileAttributes attributes);
}