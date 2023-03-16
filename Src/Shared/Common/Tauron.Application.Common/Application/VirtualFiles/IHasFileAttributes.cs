using System.IO;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IHasFileAttributes
{
    FileAttributes Attributes { get; }

    [Pure]
    IFile SetAttributes(FileAttributes attributes);
}