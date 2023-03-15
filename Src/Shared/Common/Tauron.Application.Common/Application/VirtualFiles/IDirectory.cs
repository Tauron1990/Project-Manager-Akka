using System.Collections.Generic;
using JetBrains.Annotations;
using Stl;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IDirectory : IFileSystemNode
{
    Result<IEnumerable<IDirectory>> Directories();

    Result<IEnumerable<IFile>> Files();

    Result<IFile> GetFile(in PathInfo name);

    Result<IDirectory> GetDirectory(in PathInfo name);

    Result<IDirectory> MoveTo(in PathInfo location);
}