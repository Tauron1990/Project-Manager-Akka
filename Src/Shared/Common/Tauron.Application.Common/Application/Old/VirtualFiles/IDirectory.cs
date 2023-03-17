using System.Collections.Generic;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IDirectory : IFileSystemNode<IDirectory>
{
    [Pure]
    Result<IEnumerable<IDirectory>> Directories();

    [Pure]
    Result<IEnumerable<IFile>> Files();

    [Pure]
    Result<IFile> GetFile(in PathInfo name);

    [Pure]
    Result<IDirectory> GetDirectory(in PathInfo name);

    [Pure]
    Result<IDirectory> MoveTo(in PathInfo location);
}