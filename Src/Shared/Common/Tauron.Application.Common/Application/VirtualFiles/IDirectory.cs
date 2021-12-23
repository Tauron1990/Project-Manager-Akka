using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles;

[PublicAPI]
public interface IDirectory : IFileSystemNode
{
    IEnumerable<IDirectory> Directories { get; }

    IEnumerable<IFile> Files { get; }

    IFile GetFile(PathInfo name);

    IDirectory GetDirectory(PathInfo name);

    IDirectory MoveTo(PathInfo location);
}