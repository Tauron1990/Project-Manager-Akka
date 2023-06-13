using System.IO;

namespace Tauron.Application.VirtualFiles;

public interface IHasFileAttributes
{
    FileAttributes Attributes { get; }

    Result SetFileAttributes(FileAttributes attributes);
}