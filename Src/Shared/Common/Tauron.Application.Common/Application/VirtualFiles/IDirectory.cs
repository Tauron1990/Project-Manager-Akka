using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IDirectory : IFileSystemNode
{
    Result<IEnumerable<IDirectory>> Directories { get; }

    Result<IEnumerable<IFile>> Files { get; }

    Result<IFile> GetFile(in PathInfo name);

    Result<IDirectory> GetDirectory(in PathInfo name);

    Result<IDirectory> MoveTo(PathInfo location);
}